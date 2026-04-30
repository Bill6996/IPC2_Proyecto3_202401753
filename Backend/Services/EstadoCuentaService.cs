using System.Globalization;
using Backend.Models;

namespace Backend.Services
{
    public class EstadoCuentaService
    {
        private readonly XmlDataService _data;
        public EstadoCuentaService(XmlDataService data) => _data = data;

        public object GetEstadoCuenta(string? nit)
        {
            var clientes = _data.GetClientes();
            var facturas = _data.GetFacturas();
            var pagos = _data.GetPagos();
            var bancos = _data.GetBancos();

            var clientesFiltrados = string.IsNullOrWhiteSpace(nit)
                ? clientes.OrderBy(c => c.NIT).ToList()
                : clientes.Where(c =>
                    c.NIT.Equals(nit.Trim().ToUpper(),
                    StringComparison.OrdinalIgnoreCase)).ToList();

            var resultado = new List<object>();

            foreach (var cliente in clientesFiltrados)
            {
                var transacciones = new List<object>();

                // Agregar facturas como cargos
                foreach (var f in facturas.Where(f => f.NITcliente == cliente.NIT))
                {
                    transacciones.Add(new
                    {
                        f.Fecha,
                        Tipo = "cargo",
                        Descripcion = f.NumeroFactura,
                        Monto = f.Valor,
                        Pendiente = f.SaldoPendiente
                    });
                }

                // Agregar pagos como abonos
                foreach (var p in pagos.Where(p => p.NITcliente == cliente.NIT))
                {
                    var banco = bancos.FirstOrDefault(b => b.Codigo == p.CodigoBanco);
                    transacciones.Add(new
                    {
                        p.Fecha,
                        Tipo = "abono",
                        Descripcion = banco?.Nombre ?? $"Banco {p.CodigoBanco}",
                        Monto = p.Valor,
                        Pendiente = 0.0
                    });
                }

                // Ordenar de más reciente a más antigua
                var ordenadas = transacciones
                    .OrderByDescending(t =>
                    {
                        dynamic d = t;
                        return TransaccionService.TryParseFecha((string)d.Fecha, out var dt)
                            ? dt : DateTime.MinValue;
                    }).ToList();

                double saldoDeuda = facturas
                    .Where(f => f.NITcliente == cliente.NIT)
                    .Sum(f => f.SaldoPendiente);

                resultado.Add(new
                {
                    cliente.NIT,
                    cliente.Nombre,
                    SaldoActual = Math.Round(saldoDeuda, 2),
                    SaldoAFavor = Math.Round(cliente.SaldoAFavor, 2),
                    TotalFacturas = facturas.Count(f => f.NITcliente == cliente.NIT),
                    TotalPagos = pagos.Count(p => p.NITcliente == cliente.NIT),
                    Transacciones = ordenadas
                });
            }

            return resultado;
        }

        public object GetResumenPagos(int mes, int anio)
        {
            var pagos = _data.GetPagos();
            var bancos = _data.GetBancos();

            // Últimos 3 meses de forma descendente
            var meses = new List<(int m, int a)>();
            for (int i = 0; i < 3; i++)
            {
                int m = mes - i, a = anio;
                while (m <= 0) { m += 12; a--; }
                meses.Add((m, a));
            }

            var resultado = new List<object>();

            foreach (var banco in bancos)
            {
                var detalle = meses.Select(x =>
                {
                    double total = pagos
                        .Where(p => p.CodigoBanco == banco.Codigo
                            && TransaccionService.TryParseFecha(p.Fecha, out var d)
                            && d.Month == x.m && d.Year == x.a)
                        .Sum(p => p.Valor);
                    return new { Mes = x.m, Anio = x.a, Total = Math.Round(total, 2) };
                }).ToList();

                resultado.Add(new
                {
                    banco.Codigo,
                    banco.Nombre,
                    Meses = detalle,
                    TotalBanco = Math.Round(detalle.Sum(d => d.Total), 2)
                });
            }

            return resultado;
        }
    }
}