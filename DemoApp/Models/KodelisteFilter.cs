using System.Xml.Serialization;

namespace DemoApp.Models;

[Serializable]
[XmlRoot(ElementName = "kodelistefilter")]
public class KodelisteFilter
{
    //  Håndterer slike i rotelementet:
    /* <kodelistefilter>
					<filter>
						<kodeliste_id>BUF_1E902327-85D7-4C70-B63C-893F4542757C</kodeliste_id>
						<filterliste_id>BUF_D960BB0E-3A3E-4F81-AA5A-DDEA3F51061E</filterliste_id>
						<filterverdi>1</filterverdi>
					</filter>
				</kodelistefilter>
    */

    [XmlAttribute(AttributeName = "xmlns")]
    public string? xmlns { get; set; }

    [XmlElement]
    public List<Filter> filter { get; set; } = [];
}

[Serializable]
public class Filter
{
    [XmlElement]
    public string? kodeliste_id { get; set; }
    [XmlElement]
    public string? filterliste_id { get; set; }
    [XmlElement]
    public string? filterverdi { get; set; }
}
