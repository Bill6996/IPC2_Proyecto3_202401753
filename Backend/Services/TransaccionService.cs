using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Backend.Models;

namespace Backend.Services
{
    public class TransaccionService
    {
        private readonly XmlDataService _data;

        public TransaccionService(XmlDataService data) => _data = data;

        public (int nf, int fd, int fe, int np, int pd, int pe)
            ProcesarTransacciones(string xmlContent)
        {
            var doc = XDocument.Parse(xmlContent);
            var facturas = _data.GetFacturas();
            var pagos = _data.GetPagos();
            var clientes = _data.GetClientes();
            var bancos = _data.GetBancos();

            int nf = 0, fd = 0, fe = 0, np = 0, pd = 0, pe = 0;

            // ── Procesar Facturas ──────────────────────────────────────────
            foreach (var el in doc.Descendants("factura"))
            {
                var numeroFactura = el.Element("numeroFactura")?.Value?.Trim() ?? "";
                var nitCliente = ConfigService.LimpiarNIT(el.Element("NITcliente")?.Value?.Trim() ?? "");
                var fechaStr = ExtraerFecha(el.Element("fecha")?.Value?.Trim() ?? "");
                var valorStr = el.Element("valor")?.Value?.Trim() ?? "";

                if (string.IsNullOrEmpty(numeroFactura) || string.IsNullOrEmpty(nitCliente))
                { fe++; continue; }

                // Factura duplicada
                if (facturas.Any(f => f.NumeroFactura == numeroFactura))
                { fd++; continue; }

                // Cliente no existe
                var cliente = clientes.FirstOrDefault(c => c.NIT == nitCliente);
                if (cliente == null) { fe++; continue; }

                // Fecha inválida
                if (!DateTime.TryParseExact(fechaStr, "dd/MM/yyyy",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                { fe++; continue; }

                // Valor inválido
                if (!double.TryParse(valorStr, NumberStyles.Any,
                    CultureInfo.InvariantCulture, out double valor) || valor <= 0)
                { fe++; continue; }

                // Aplicar saldo a favor del cliente si tiene
                double saldoPendiente = valor;
                if (cliente.SaldoAFavor > 0)
                {
                    double aplicado = Math.Min(cliente.SaldoAFavor, saldoPendiente);
                    cliente.SaldoAFavor -= aplicado;
                    saldoPendiente -= aplicado;
                }

                facturas.Add(new Factura
                {
                    NumeroFactura = numeroFactura,
                    NITcliente = nitCliente,
                    Fecha = fechaStr,
                    Valor = valor,
                    SaldoPendiente = saldoPendiente
                });
                nf++;
            }

            // ── Procesar Pagos ─────────────────────────────────────────────
            foreach (var el in doc.Descendants("pago"))
            {
                var codigoStr = el.Element("codigoBanco")?.Value?.Trim() ?? "";
                var fechaStr = ExtraerFecha(el.Element("fecha")?.Value?.Trim() ?? "");
                var nitCliente = ConfigService.LimpiarNIT(el.Element("NITcliente")?.Value?.Trim() ?? "");
                var valorStr = el.Element("valor")?.Value?.Trim() ?? "";

                if (!int.TryParse(codigoStr, out int codigoBanco)) { pe++; continue; }
                if (bancos.All(b => b.Codigo != codigoBanco)) { pe++; continue; }

                var cliente = clientes.FirstOrDefault(c => c.NIT == nitCliente);
                if (cliente == null) { pe++; continue; }

                if (!DateTime.TryParseExact(fechaStr, "dd/MM/yyyy",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                { pe++; continue; }

                if (!double.TryParse(valorStr, NumberStyles.Any,
                    CultureInfo.InvariantCulture, out double valor) || valor <= 0)
                { pe++; continue; }

                // Pago duplicado (mismo banco + cliente + fecha + monto)
                if (pagos.Any(p => p.CodigoBanco == codigoBanco
                    && p.NITcliente == nitCliente
                    && p.Fecha == fechaStr
                    && p.Valor == valor))
                { pd++; continue; }

                // Abonar a facturas más antiguas primero
                double resto = valor;
                var facturasCliente = facturas
                    .Where(f => f.NITcliente == nitCliente && f.SaldoPendiente > 0)
                    .OrderBy(f => DateTime.ParseExact(f.Fecha, "dd/MM/yyyy", CultureInfo.InvariantCulture))
                    .ToList();

                foreach (var factura in facturasCliente)
                {
                    if (resto <= 0) break;
                    double aplicado = Math.Min(resto, factura.SaldoPendiente);
                    factura.SaldoPendiente -= aplicado;
                    resto -= aplicado;
                }

                // Si sobró dinero → saldo a favor del cliente
                if (resto > 0)
                    cliente.SaldoAFavor += resto;

                pagos.Add(new Pago
                {
                    CodigoBanco = codigoBanco,
                    Fecha = fechaStr,
                    NITcliente = nitCliente,
                    Valor = valor
                });
                np++;
            }

            _data.SaveFacturas(facturas);
            _data.SavePagos(pagos);
            _data.SaveClientes(clientes);

            return (nf, fd, fe, np, pd, pe);
        }

        // Expresión regular: extrae el patrón dd/mm/yyyy de cualquier texto
        private static string ExtraerFecha(string texto)
        {
            var match = Regex.Match(texto, @"\d{2}/\d{2}/\d{4}");
            return match.Success ? match.Value : texto;
        }
    }
}