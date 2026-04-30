using System.Xml.Serialization;

namespace Backend.Models
{
    public class Cliente
    {
        [XmlElement("NIT")]
        public string NIT { get; set; } = "";

        [XmlElement("nombre")]
        public string Nombre { get; set; } = "";

        [XmlElement("saldo")]
        public double SaldoAFavor { get; set; } = 0.0;
    }
}