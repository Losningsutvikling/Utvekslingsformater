using System.Xml.Serialization;

namespace DemoApp.Models
{
    [Serializable]
    public class Kodeliste
    {
        [XmlAttribute]
        public string? id { get; set; }
        [XmlAttribute]
        public string? navn { get; set; }
        [XmlAttribute]
        public string? utdragfra { get; set; }
        [XmlAttribute]
        public string? kobletKodeliste_1 { get; set; }
        [XmlAttribute]
        public string? kobletKodeliste_2 { get; set; }

        [XmlElement(ElementName = "kode")]
        public Kode[]? koder { get; set; }
    }
}
