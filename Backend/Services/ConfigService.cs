using System.Text.RegularExpressions;
using System.Xml.Linq;
using Backend.Models;

namespace Backend.Services
{
    public class ConfigService
    {
        private readonly XmlDataService _data;

        public ConfigService(XmlDataService data) => _data = data;

        public (int cc, int ca, int bc, int ba) ProcesarConfig(string xmlContent)
        {
            var doc = XDocument.Parse(xmlContent);
            var clientes = _data.GetClientes();
            var bancos = _data.GetBancos();

            int cc = 0, ca = 0, bc = 0, ba = 0;

            foreach (var el in doc.Descendants("cliente"))
            {
                var nit = LimpiarNIT(el.Element("NIT")?.Value?.Trim() ?? "");
                var nombre = el.Element("nombre")?.Value?.Trim() ?? "";
                if (string.IsNullOrEmpty(nit)) continue;

                var existente = clientes.FirstOrDefault(c => c.NIT == nit);
                if (existente != null) { existente.Nombre = nombre; ca++; }
                else { clientes.Add(new Cliente { NIT = nit, Nombre = nombre }); cc++; }
            }

            foreach (var el in doc.Descendants("banco"))
            {
                var codigoStr = el.Element("codigo")?.Value?.Trim() ?? "";
                var nombre = el.Element("nombre")?.Value?.Trim() ?? "";
                if (!int.TryParse(codigoStr, out int codigo)) continue;

                var existente = bancos.FirstOrDefault(b => b.Codigo == codigo);
                if (existente != null) { existente.Nombre = nombre; ba++; }
                else { bancos.Add(new Banco { Codigo = codigo, Nombre = nombre }); bc++; }
            }

            _data.SaveClientes(clientes);
            _data.SaveBancos(bancos);

            return (cc, ca, bc, ba);
        }

        // Expresión regular: extrae solo caracteres válidos de un NIT
        public static string LimpiarNIT(string nit) =>
            Regex.Replace(nit, @"[^a-zA-Z0-9\-]", "").Trim().ToUpper();
    }
}