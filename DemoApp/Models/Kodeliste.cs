using System.Xml.Serialization;

namespace DemoApp.Models
{
    [Serializable]
    public class Kodeliste
    {
        [XmlAttribute]
        public string? navn { get; set; }
        [XmlAttribute]
        public string? utdragfra { get; set; }
        [XmlElement(ElementName = "kode")]
        public Kode[]? koder { get; set; }
    }
}
