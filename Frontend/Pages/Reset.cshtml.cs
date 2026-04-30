using Frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages
{
    public class ResetModel : PageModel
    {
        private readonly ApiService _api;
        public ResetModel(ApiService api) => _api = api;
        public string? Mensaje { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            await _api.LimpiarDatos();
            Mensaje = "✅ Sistema reseteado correctamente. Todos los datos han sido eliminados.";
            return Page();
        }
    }
}