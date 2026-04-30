using System.Collections.Generic;
using System.Xml.Serialization;

namespace Backend.Models
{
    [XmlRoot("clientes")]
    public class ClienteList
    {
        [XmlElement("cliente")]
        public List<Cliente> Clientes { get; set; } = new();
    }

    [XmlRoot("bancos")]
    public class BancoList
    {
        [XmlElement("banco")]
        public List<Banco> Bancos { get; set; } = new();
    }

    [XmlRoot("facturas")]
    public class FacturaList
    {
        [XmlElement("factura")]
        public List<Factura> Facturas { get; set; } = new();
    }

    [XmlRoot("pagos")]
    public class PagoList
    {
        [XmlElement("pago")]
        public List<Pago> Pagos { get; set; } = new();
    }
}