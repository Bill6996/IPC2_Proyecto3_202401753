using Frontend.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Frontend.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApiService _api;
        public IndexModel(ApiService api) => _api = api;

        public int TotalClientes { get; set; }
        public int TotalBancos { get; set; }
        public int TotalFacturas { get; set; }
        public int TotalPagos { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                var jsonC = await _api.GetClientes();
                TotalClientes = JsonSerializer.Deserialize<List<object>>(jsonC, opts)?.Count ?? 0;

                var jsonB = await _api.GetBancos();
                TotalBancos = JsonSerializer.Deserialize<List<object>>(jsonB, opts)?.Count ?? 0;

                // Facturas y pagos via estado de cuenta general
                var jsonE = await _api.GetEstadoCuenta("");
                var clientes = JsonSerializer.Deserialize<List<JsonElement>>(jsonE, opts) ?? new();
                TotalFacturas = clientes.Sum(c =>
                    c.TryGetProperty("transacciones", out var t)
                        ? t.EnumerateArray().Count(x =>
                            x.TryGetProperty("tipo", out var tipo) && tipo.GetString() == "cargo")
                        : 0);
                TotalPagos = clientes.Sum(c =>
                    c.TryGetProperty("transacciones", out var t)
                        ? t.EnumerateArray().Count(x =>
                            x.TryGetProperty("tipo", out var tipo) && tipo.GetString() == "abono")
                        : 0);
            }
            catch { /* Si el backend no está disponible, muestra 0s */ }
        }
    }
}