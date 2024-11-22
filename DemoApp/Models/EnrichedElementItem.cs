using System.Xml.Schema;

namespace DemoApp.Models;

public class EnrichedElement
{
    public XmlSchemaAnnotated Element { get; set; }
    public int OrigSort { get; set; }
    public int SortOrder { get; set; }
}
