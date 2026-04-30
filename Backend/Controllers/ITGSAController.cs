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
        private readonly PdfService _pdfSvc;

        public ITGSAController(
            ConfigService configSvc,
            TransaccionService transacSvc,
            EstadoCuentaService estadoSvc,
            XmlDataService dataSvc,
            PdfService pdfSvc)
        {
            _configSvc = configSvc;
            _transacSvc = transacSvc;
            _estadoSvc = estadoSvc;
            _dataSvc = dataSvc;
            _pdfSvc = pdfSvc;
        }

        // ── POST /api/grabarConfiguracion ──────────────────────────────────
        [HttpPost("grabarConfiguracion")]
        public async Task<IActionResult> GrabarConfiguracion(IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
                return BadRequest(RespuestaError("No se recibió ningún archivo."));

            try
            {
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
            catch (Exception ex)
            {
                return BadRequest(RespuestaError(ex.Message));
            }
        }

        // ── POST /api/grabarTransaccion ────────────────────────────────────
        [HttpPost("grabarTransaccion")]
        public async Task<IActionResult> GrabarTransaccion(IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
                return BadRequest(RespuestaError("No se recibió ningún archivo."));

            try
            {
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
            catch (Exception ex)
            {
                return BadRequest(RespuestaError(ex.Message));
            }
        }

        // ── POST /api/limipiarDatos ────────────────────────────────────────
        [HttpPost("limipiarDatos")]
        public IActionResult LimpiarDatos()
        {
            _dataSvc.LimpiarTodo();
            var respuesta = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement("respuesta",
                    new XElement("mensaje", "Sistema reiniciado correctamente.")));
            return Content(respuesta.ToString(), "application/xml", Encoding.UTF8);
        }

        // ── GET /api/devolverEstadoCuenta?nit=xxx ─────────────────────────
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

        // ── GET /api/pdfEstadoCuenta?nit=xxx ──────────────────────────────
        [HttpGet("pdfEstadoCuenta")]
        public IActionResult PdfEstadoCuenta([FromQuery] string? nit)
        {
            try
            {
                var bytes = _pdfSvc.GenerarEstadoCuentaPdf(nit);
                return File(bytes, "application/pdf",
                    $"EstadoCuenta_{nit ?? "todos"}_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(RespuestaError(ex.Message));
            }
        }

        // ── GET /api/pdfResumenPagos?mes=3&anio=2024 ──────────────────────
        [HttpGet("pdfResumenPagos")]
        public IActionResult PdfResumenPagos([FromQuery] int mes, [FromQuery] int anio)
        {
            try
            {
                var bytes = _pdfSvc.GenerarResumenPagosPdf(mes, anio);
                return File(bytes, "application/pdf",
                    $"ResumenPagos_{mes:D2}_{anio}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(RespuestaError(ex.Message));
            }
        }

        // ── GET /api/clientes ─────────────────────────────────────────────
        [HttpGet("clientes")]
        public IActionResult GetClientes()
        {
            var clientes = _dataSvc.GetClientes().OrderBy(c => c.NIT);
            return Ok(clientes);
        }

        // ── GET /api/bancos ───────────────────────────────────────────────
        [HttpGet("bancos")]
        public IActionResult GetBancos()
        {
            var bancos = _dataSvc.GetBancos().OrderBy(b => b.Codigo);
            return Ok(bancos);
        }

        // ── GET /api/facturas?nit=xxx ─────────────────────────────────────
        [HttpGet("facturas")]
        public IActionResult GetFacturas([FromQuery] string? nit)
        {
            var facturas = _dataSvc.GetFacturas()
                .Where(f => string.IsNullOrWhiteSpace(nit) || f.NITcliente == nit.ToUpper().Trim())
                .OrderBy(f => f.Fecha)
                .ToList();
            return Ok(facturas);
        }

        // ── GET /api/pagos?nit=xxx ────────────────────────────────────────
        [HttpGet("pagos")]
        public IActionResult GetPagos([FromQuery] string? nit)
        {
            var pagos = _dataSvc.GetPagos()
                .Where(p => string.IsNullOrWhiteSpace(nit) || p.NITcliente == nit.ToUpper().Trim())
                .OrderBy(p => p.Fecha)
                .ToList();
            return Ok(pagos);
        }

        // ─────────────────────────────────────────────────────────────────
        private static string RespuestaError(string mensaje) =>
            $"<?xml version=\"1.0\"?><error><mensaje>{mensaje}</mensaje></error>";
    


        // ── GET /api/estadisticas ─────────────────────────────────────────
        [HttpGet("estadisticas")]
        public IActionResult GetEstadisticas()
        {
            var clientes = _dataSvc.GetClientes();
            var bancos = _dataSvc.GetBancos();
            var facturas = _dataSvc.GetFacturas();
            var pagos = _dataSvc.GetPagos();

            double totalFacturado = facturas.Sum(f => f.Valor);
            double totalPagado = pagos.Sum(p => p.Valor);
            double totalPendiente = facturas.Sum(f => f.SaldoPendiente);
            double totalAFavor = clientes.Sum(c => c.SaldoAFavor);

            return Ok(new
            {
                Clientes = clientes.Count,
                Bancos = bancos.Count,
                Facturas = facturas.Count,
                Pagos = pagos.Count,
                TotalFacturado = Math.Round(totalFacturado, 2),
                TotalPagado = Math.Round(totalPagado, 2),
                TotalPendiente = Math.Round(totalPendiente, 2),
                TotalSaldoAFavor = Math.Round(totalAFavor, 2)
            });
        }

    }
}