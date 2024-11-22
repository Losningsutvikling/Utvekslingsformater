using System.Xml;

namespace DemoApp.Models;

public static class Hack
{
    public static KodelisteFilter XmlToKodelistefilter(string xml)
    {
        KodelisteFilter result = new KodelisteFilter();
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(xml);
        if (doc.ChildNodes.Count == 1)
        {
            foreach (var filter in doc.ChildNodes[0]!)
            {
                if (filter is XmlElement el)
                {
                    Filter filterObject = new()
                    {
                        kodeliste_id = el["kodeliste_id"]?.InnerText,
                        filterliste_id = el["filterliste_id"]?.InnerText,
                        filterverdi = el["filterverdi"]?.InnerText
                    };

                    result.filter.Add(filterObject);
                }
            }
        }
        return result;
    }
}
