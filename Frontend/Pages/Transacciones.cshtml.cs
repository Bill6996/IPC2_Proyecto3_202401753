using Frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Xml.Linq;

namespace Frontend.Pages
{
    public class TransaccionesModel : PageModel
    {
        private readonly ApiService _api;
        public TransaccionesModel(ApiService api) => _api = api;

        public TransacRespuesta? Respuesta { get; set; }
        public string RespuestaXml { get; set; } = "";
        public string Error { get; set; } = "";

        public async Task<IActionResult> OnPostAsync(IFormFile archivo)
        {
            try
            {
                RespuestaXml = await _api.GrabarTransaccion(archivo);
                var doc = XDocument.Parse(RespuestaXml);
                Respuesta = new TransacRespuesta
                {
                    NuevasFacturas = int.Parse(doc.Descendants("nuevasFacturas").First().Value),
                    FacturasDuplicadas = int.Parse(doc.Descendants("facturasDuplicadas").First().Value),
                    FacturasConError = int.Parse(doc.Descendants("facturasConError").First().Value),
                    NuevosPagos = int.Parse(doc.Descendants("nuevosPagos").First().Value),
                    PagosDuplicados = int.Parse(doc.Descendants("pagosDuplicados").First().Value),
                    PagosConError = int.Parse(doc.Descendants("pagosConError").First().Value)
                };
            }
            catch (Exception ex) { Error = $"Error: {ex.Message}"; }
            return Page();
        }
    }

    public class TransacRespuesta
    {
        public int NuevasFacturas { get; set; }
        public int FacturasDuplicadas { get; set; }
        public int FacturasConError { get; set; }
        public int NuevosPagos { get; set; }
        public int PagosDuplicados { get; set; }
        public int PagosConError { get; set; }
    }
}