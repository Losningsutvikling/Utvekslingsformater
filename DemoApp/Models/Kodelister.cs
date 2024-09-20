using System.Xml.Serialization;

namespace DemoApp.Models
{
    [Serializable]
    [XmlRoot(ElementName = "kodelister")]//, Namespace = "kodelister.bufdir.no", DataType = "string", IsNullable = true)]
    public class Kodelister
    {
        [XmlAttribute(AttributeName = "nmsp")]
        public string? nmsp { get; set; }

        [XmlElement(ElementName = "kodeliste")]
        public Kodeliste[]? kodelister { get; set; }
    }
}
