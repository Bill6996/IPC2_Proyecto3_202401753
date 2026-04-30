using Frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Xml.Linq;

namespace Frontend.Pages
{
    public class ConfiguracionModel : PageModel
    {
        private readonly ApiService _api;
        public ConfiguracionModel(ApiService api) => _api = api;

        public ConfigRespuesta? Respuesta { get; set; }
        public string RespuestaXml { get; set; } = "";
        public string Error { get; set; } = "";

        public async Task<IActionResult> OnPostAsync(IFormFile archivo)
        {
            try
            {
                RespuestaXml = await _api.GrabarConfiguracion(archivo);
                var doc = XDocument.Parse(RespuestaXml);
                Respuesta = new ConfigRespuesta
                {
                    ClientesCreados = int.Parse(doc.Descendants("creados").First().Value),
                    ClientesActualizados = int.Parse(doc.Descendants("actualizados").First().Value),
                    BancosCreados = int.Parse(doc.Descendants("creados").Last().Value),
                    BancosActualizados = int.Parse(doc.Descendants("actualizados").Last().Value)
                };
            }
            catch (Exception ex) { Error = $"Error: {ex.Message}"; }
            return Page();
        }
    }

    public class ConfigRespuesta
    {
        public int ClientesCreados { get; set; }
        public int ClientesActualizados { get; set; }
        public int BancosCreados { get; set; }
        public int BancosActualizados { get; set; }
    }
}