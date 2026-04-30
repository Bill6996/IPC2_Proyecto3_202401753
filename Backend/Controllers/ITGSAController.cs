using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Xml.Linq;
using Backend.Services;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api")]
    public class ITGSAController : ControllerBase
    {
        private readonly ConfigService _configSvc;
        private readonly TransaccionService _transacSvc;
        private readonly EstadoCuentaService _estadoSvc;
        private readonly XmlDataService _dataSvc;

        public ITGSAController(
            ConfigService configSvc,
            TransaccionService transacSvc,
            EstadoCuentaService estadoSvc,
            XmlDataService dataSvc)
        {
            _configSvc = configSvc;
            _transacSvc = transacSvc;
            _estadoSvc = estadoSvc;
            _dataSvc = dataSvc;
        }

        // ── POST /api/grabarConfiguracion ──────────────────────────────────
        [HttpPost("grabarConfiguracion")]
        public async Task<IActionResult> GrabarConfiguracion(IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
                return BadRequest("No se recibió ningún archivo.");

            string xmlContent;
            using (var reader = new StreamReader(archivo.OpenReadStream()))
                xmlContent = await reader.ReadToEndAsync();

            var (cc, ca, bc, ba) = _configSvc.ProcesarConfig(xmlContent);

            var respuesta = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement("respuesta",
                    new XElement("clientes",
                        new XElement("creados", cc),
                        new XElement("actualizados", ca)),
                    new XElement("bancos",
                        new XElement("creados", bc),
                        new XElement("actualizados", ba))));

            return Content(respuesta.ToString(), "application/xml", Encoding.UTF8);
        }

        // ── POST /api/grabarTransaccion ────────────────────────────────────
        [HttpPost("grabarTransaccion")]
        public async Task<IActionResult> GrabarTransaccion(IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
                return BadRequest("No se recibió ningún archivo.");

            string xmlContent;
            using (var reader = new StreamReader(archivo.OpenReadStream()))
                xmlContent = await reader.ReadToEndAsync();

            var (nf, fd, fe, np, pd, pe) = _transacSvc.ProcesarTransacciones(xmlContent);

            var respuesta = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement("transacciones",
                    new XElement("facturas",
                        new XElement("nuevasFacturas", nf),
                        new XElement("facturasDuplicadas", fd),
                        new XElement("facturasConError", fe)),
                    new XElement("pagos",
                        new XElement("nuevosPagos", np),
                        new XElement("pagosDuplicados", pd),
                        new XElement("pagosConError", pe))));

            return Content(respuesta.ToString(), "application/xml", Encoding.UTF8);
        }

        // ── POST /api/limipiarDatos ────────────────────────────────────────
        [HttpPost("limipiarDatos")]
        public IActionResult LimpiarDatos()
        {
            _dataSvc.LimpiarTodo();
            return Ok(new { mensaje = "Sistema reiniciado correctamente." });
        }

        // ── GET /api/devolverEstadoCuenta?nit=xxx  (nit es opcional) ──────
        [HttpGet("devolverEstadoCuenta")]
        public IActionResult DevolverEstadoCuenta([FromQuery] string? nit)
        {
            var resultado = _estadoSvc.GetEstadoCuenta(nit);
            return Ok(resultado);
        }

        // ── GET /api/devolverResumenPagos?mes=3&anio=2024 ─────────────────
        [HttpGet("devolverResumenPagos")]
        public IActionResult DevolverResumenPagos([FromQuery] int mes, [FromQuery] int anio)
        {
            var resultado = _estadoSvc.GetResumenPagos(mes, anio);
            return Ok(resultado);
        }
    }
}