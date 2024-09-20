using System.Xml.Serialization;

namespace DemoApp.Models
{
    [Serializable]
    public class Kode
    {
        [XmlAttribute]
        public string? verdi { get; set; }
        [XmlAttribute]
        public string? tekst { get; set; }
    }
}
