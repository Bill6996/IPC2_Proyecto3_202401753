using Frontend.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Frontend.Pages
{
    public class IngresosModel : PageModel
    {
        private readonly ApiService _api;
        public IngresosModel(ApiService api) => _api = api;

        public int Mes { get; set; } = DateTime.Now.Month;
        public int Anio { get; set; } = DateTime.Now.Year;
        public List<BancoIngresosVM>? Datos { get; set; }
        public string? ChartJson { get; set; }

        public async Task OnGetAsync(int? mes, int? anio)
        {
            Mes = mes ?? DateTime.Now.Month;
            Anio = anio ?? DateTime.Now.Year;

            if (Request.Query.ContainsKey("mes"))
            {
                var json = await _api.GetResumenPagos(Mes, Anio);
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                Datos = JsonSerializer.Deserialize<List<BancoIngresosVM>>(json, opts) ?? new();
                ChartJson = JsonSerializer.Serialize(Datos.Select(b => new
                {
                    nombre = b.Nombre,
                    meses = b.Meses.Select(m => new { m.Mes, m.Anio, m.Total })
                }));
            }
        }
    }

    public class BancoIngresosVM
    {
        public int Codigo { get; set; }
        public string Nombre { get; set; } = "";
        public List<MesIngresosVM> Meses { get; set; } = new();
    }

    public class MesIngresosVM
    {
        public int Mes { get; set; }
        public int Anio { get; set; }
        public double Total { get; set; }
    }
}