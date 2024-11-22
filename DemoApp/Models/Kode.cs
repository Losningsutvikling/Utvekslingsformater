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
        [XmlAttribute]
        public string? beskrivelse { get; set; }
        [XmlAttribute]
        public string? kobletKodeliste_1 { get; set; }
        [XmlAttribute]
        public string? kobletKodeliste_2 { get; set; }
    }
}
