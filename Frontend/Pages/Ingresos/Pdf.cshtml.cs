using Frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages.Ingresos
{
    public class PdfModel : PageModel
    {
        private readonly ApiService _api;

        public PdfModel(ApiService api) => _api = api;

        public async Task<IActionResult> OnGetAsync(int mes, int anio)
        {
            try
            {
                // Esta llamada al API generará el PDF de los ingresos por banco
                var bytes = await _api.DescargarPdfResumenPagos(mes, anio);
                var fileName = $"ResumenPagos_{mes:D2}_{anio}.pdf";
                return File(bytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                // Si falla, guardamos el error y volvemos a la pantalla de Ingresos
                TempData["Error"] = ex.Message;
                return RedirectToPage("/Ingresos");
            }
        }
    }
}