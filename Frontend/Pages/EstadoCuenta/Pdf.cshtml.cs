using Frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages.EstadoCuenta
{
    public class PdfModel : PageModel
    {
        private readonly ApiService _api;

        public PdfModel(ApiService api) => _api = api;

        public async Task<IActionResult> OnGetAsync(string? nit)
        {
            try
            {
                var bytes = await _api.DescargarPdfEstadoCuenta(nit);
                var fileName = $"EstadoCuenta_{nit ?? "todos"}_{DateTime.Now:yyyyMMdd}.pdf";
                return File(bytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToPage("/EstadoCuenta");
            }
        }
    }
}