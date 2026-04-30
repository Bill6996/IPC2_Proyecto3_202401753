using Frontend.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Frontend.Pages
{
    public class StatsVM
    {
        public int Clientes { get; set; }
        public int Bancos { get; set; }
        public int Facturas { get; set; }
        public int Pagos { get; set; }
        public double TotalFacturado { get; set; }
        public double TotalPagado { get; set; }
        public double TotalPendiente { get; set; }
        public double TotalSaldoAFavor { get; set; }
    }

    public class IndexModel : PageModel
    {
        private readonly ApiService _api;
        public IndexModel(ApiService api) => _api = api;

        public StatsVM Stats { get; set; } = new();

        public async Task OnGetAsync()
        {
            try
            {
                var json = await _api.GetEstadisticas();
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                Stats = JsonSerializer.Deserialize<StatsVM>(json, opts) ?? new();
            }
            catch { Stats = new(); }
        }
    }
}