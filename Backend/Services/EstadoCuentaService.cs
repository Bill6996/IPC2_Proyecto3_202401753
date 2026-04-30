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
                ? clientes.OrderBy(c => c.NIT)
                : clientes.Where(c => c.NIT == nit.Trim().ToUpper());

            var resultado = new List<object>();

            foreach (var cliente in clientesFiltrados)
            {
                var transacciones = new List<object>();

                foreach (var f in facturas.Where(f => f.NITcliente == cliente.NIT))
                    transacciones.Add(new
                    {
                        f.Fecha,
                        Tipo = "cargo",
                        Descripcion = $"Fact. # {f.NumeroFactura}",
                        Monto = f.Valor
                    });

                foreach (var p in pagos.Where(p => p.NITcliente == cliente.NIT))
                {
                    var banco = bancos.FirstOrDefault(b => b.Codigo == p.CodigoBanco);
                    transacciones.Add(new
                    {
                        p.Fecha,
                        Tipo = "abono",
                        Descripcion = banco?.Nombre ?? p.CodigoBanco.ToString(),
                        Monto = p.Valor
                    });
                }

                var ordenadas = transacciones
                    .OrderByDescending(t => {
                        dynamic d = t;
                        return DateTime.TryParseExact((string)d.Fecha, "dd/MM/yyyy",
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt)
                            ? dt : DateTime.MinValue;
                    }).ToList();

                double saldoDeuda = facturas
                    .Where(f => f.NITcliente == cliente.NIT)
                    .Sum(f => f.SaldoPendiente);

                resultado.Add(new
                {
                    cliente.NIT,
                    cliente.Nombre,
                    SaldoActual = saldoDeuda,
                    SaldoAFavor = cliente.SaldoAFavor,
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
                var detalle = meses.Select(x => {
                    double total = pagos
                        .Where(p => p.CodigoBanco == banco.Codigo)
                        .Where(p => DateTime.TryParseExact(p.Fecha, "dd/MM/yyyy",
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)
                            && d.Month == x.m && d.Year == x.a)
                        .Sum(p => p.Valor);
                    return new { Mes = x.m, Anio = x.a, Total = total };
                }).ToList();

                resultado.Add(new
                {
                    banco.Codigo,
                    banco.Nombre,
                    Meses = detalle
                });
            }

            return resultado;
        }
    }
}