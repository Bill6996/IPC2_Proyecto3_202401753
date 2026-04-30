using System.Xml.Serialization;

namespace Backend.Models
{
    public class Pago
    {
        [XmlElement("codigoBanco")]
        public int CodigoBanco { get; set; }

        [XmlElement("fecha")]
        public string Fecha { get; set; } = "";

        [XmlElement("NITcliente")]
        public string NITcliente { get; set; } = "";

        [XmlElement("valor")]
        public double Valor { get; set; }
    }
}