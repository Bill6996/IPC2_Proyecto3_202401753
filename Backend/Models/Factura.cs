using System.Xml.Serialization;

namespace Backend.Models
{
    public class Factura
    {
        [XmlElement("numeroFactura")]
        public string NumeroFactura { get; set; } = "";

        [XmlElement("NITcliente")]
        public string NITcliente { get; set; } = "";

        [XmlElement("fecha")]
        public string Fecha { get; set; } = "";

        [XmlElement("valor")]
        public double Valor { get; set; }

        [XmlElement("saldoPendiente")]
        public double SaldoPendiente { get; set; }
    }
}