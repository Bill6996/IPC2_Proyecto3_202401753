using Frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages
{
    public class ResetModel : PageModel
    {
        private readonly ApiService _api;
        public ResetModel(ApiService api) => _api = api;

        public bool Confirmando { get; set; }
        public string Mensaje { get; set; } = "";

        public void OnGet() { }

        public IActionResult OnPostConfirmar()
        {
            Confirmando = true;
            return Page();
        }

        public async Task<IActionResult> OnPostEjecutarAsync()
        {
            await _api.LimpiarDatos();
            Mensaje = "Sistema reseteado correctamente. Todos los datos han sido eliminados.";
            return Page();
        }
    }
}