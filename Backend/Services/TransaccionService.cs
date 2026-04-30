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
                var numeroFactura = LimpiarTexto(el.Element("numeroFactura")?.Value ?? "");
                var nitRaw = el.Element("NITcliente")?.Value?.Trim() ?? "";
                var nitCliente = ConfigService.LimpiarNIT(nitRaw);
                var fechaStr = ExtraerFecha(el.Element("fecha")?.Value?.Trim() ?? "");
                var valorStr = LimpiarNumero(el.Element("valor")?.Value?.Trim() ?? "");

                // Validar número de factura
                if (string.IsNullOrWhiteSpace(numeroFactura)) { fe++; continue; }

                // Validar NIT
                if (string.IsNullOrWhiteSpace(nitCliente)) { fe++; continue; }

                // Factura duplicada — verificar antes de validar cliente
                if (facturas.Any(f => f.NumeroFactura == numeroFactura)) { fd++; continue; }

                // Cliente no existe
                var cliente = clientes.FirstOrDefault(c => c.NIT == nitCliente);
                if (cliente == null) { fe++; continue; }

                // Fecha inválida
                if (!TryParseFecha(fechaStr, out _)) { fe++; continue; }

                // Valor inválido
                if (!double.TryParse(valorStr, NumberStyles.Any,
                    CultureInfo.InvariantCulture, out double valor) || valor <= 0)
                { fe++; continue; }

                // Aplicar saldo a favor del cliente
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
                var codigoStr = LimpiarNumero(el.Element("codigoBanco")?.Value?.Trim() ?? "");
                var fechaStr = ExtraerFecha(el.Element("fecha")?.Value?.Trim() ?? "");
                var nitRaw = el.Element("NITcliente")?.Value?.Trim() ?? "";
                var nitCliente = ConfigService.LimpiarNIT(nitRaw);
                var valorStr = LimpiarNumero(el.Element("valor")?.Value?.Trim() ?? "");

                // Validar código banco
                if (!int.TryParse(codigoStr, out int codigoBanco)) { pe++; continue; }
                if (bancos.All(b => b.Codigo != codigoBanco)) { pe++; continue; }

                // Validar cliente
                var cliente = clientes.FirstOrDefault(c => c.NIT == nitCliente);
                if (cliente == null) { pe++; continue; }

                // Validar fecha
                if (!TryParseFecha(fechaStr, out _)) { pe++; continue; }

                // Validar valor
                if (!double.TryParse(valorStr, NumberStyles.Any,
                    CultureInfo.InvariantCulture, out double valor) || valor <= 0)
                { pe++; continue; }

                // Pago duplicado
                if (pagos.Any(p => p.CodigoBanco == codigoBanco
                    && p.NITcliente == nitCliente
                    && p.Fecha == fechaStr
                    && Math.Abs(p.Valor - valor) < 0.001))
                { pd++; continue; }

                // Abonar a facturas más antiguas primero
                double resto = valor;
                var facturasCliente = facturas
                    .Where(f => f.NITcliente == nitCliente && f.SaldoPendiente > 0.001)
                    .OrderBy(f => DateTime.ParseExact(
                        f.Fecha, "dd/MM/yyyy", CultureInfo.InvariantCulture))
                    .ToList();

                foreach (var factura in facturasCliente)
                {
                    if (resto <= 0) break;
                    double aplicado = Math.Min(resto, factura.SaldoPendiente);
                    factura.SaldoPendiente -= aplicado;
                    resto -= aplicado;
                }

                // Sobró → saldo a favor
                if (resto > 0.001)
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

        // ── Helpers con Regex ──────────────────────────────────────────────

        // Extrae dd/mm/yyyy de cualquier string que pueda tener texto extra
        public static string ExtraerFecha(string texto)
        {
            var match = Regex.Match(texto, @"\b(\d{2}/\d{2}/\d{4})\b");
            return match.Success ? match.Value : texto.Trim();
        }

        // Extrae solo dígitos y punto decimal de un valor numérico
        public static string LimpiarNumero(string texto)
        {
            // Elimina todo excepto dígitos, punto y coma decimal
            var limpio = Regex.Replace(texto, @"[^\d.,]", "");
            // Si usa coma como decimal, la convierte a punto
            limpio = Regex.Replace(limpio, @",(\d{1,2})$", ".$1");
            return limpio;
        }

        // Limpia espacios y caracteres no imprimibles del texto
        public static string LimpiarTexto(string texto) =>
            Regex.Replace(texto.Trim(), @"\s+", " ");

        // Intenta parsear fecha en formato dd/MM/yyyy
        public static bool TryParseFecha(string fecha, out DateTime result) =>
            DateTime.TryParseExact(fecha, "dd/MM/yyyy",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
    }
}