using System.Xml.Serialization;

namespace Backend.Models
{
    public class Banco
    {
        [XmlElement("codigo")]
        public int Codigo { get; set; }

        [XmlElement("nombre")]
        public string Nombre { get; set; } = "";
    }
}