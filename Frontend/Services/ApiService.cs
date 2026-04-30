using System.Net.Http.Headers;

namespace Frontend.Services
{
    public class ApiService
    {
        private readonly IHttpClientFactory _factory;

        public ApiService(IHttpClientFactory factory) => _factory = factory;

        private HttpClient Client() => _factory.CreateClient("Backend");

        public async Task<string> GrabarConfiguracion(IFormFile archivo)
        {
            using var content = new MultipartFormDataContent();
            using var stream = archivo.OpenReadStream();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
            content.Add(fileContent, "archivo", archivo.FileName);
            var res = await Client().PostAsync("grabarConfiguracion", content);
            return await res.Content.ReadAsStringAsync();
        }

        public async Task<string> GrabarTransaccion(IFormFile archivo)
        {
            using var content = new MultipartFormDataContent();
            using var stream = archivo.OpenReadStream();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
            content.Add(fileContent, "archivo", archivo.FileName);
            var res = await Client().PostAsync("grabarTransaccion", content);
            return await res.Content.ReadAsStringAsync();
        }

        public async Task<string> LimpiarDatos()
        {
            var res = await Client().PostAsync("limipiarDatos", null);
            return await res.Content.ReadAsStringAsync();
        }

        public async Task<string> GetEstadoCuenta(string? nit)
        {
            var url = string.IsNullOrWhiteSpace(nit)
                ? "devolverEstadoCuenta"
                : $"devolverEstadoCuenta?nit={Uri.EscapeDataString(nit)}";
            var res = await Client().GetAsync(url);
            return await res.Content.ReadAsStringAsync();
        }

        public async Task<string> GetResumenPagos(int mes, int anio)
        {
            var res = await Client().GetAsync($"devolverResumenPagos?mes={mes}&anio={anio}");
            return await res.Content.ReadAsStringAsync();
        }
    }
}