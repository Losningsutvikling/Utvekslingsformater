using DemoApp.Models.Fagsystem;
using DemoApp.Pages;
using System.Xml.Schema;
namespace DemoApp.Models.ViewModels
{
    public class PropertyIteratorRendererModel(PropertyRendererModel? parent, MeldingModel melding, string xPath, XmlSchemaAnnotated prop, List<PrefilledValue> values, string iteratorElement) : PropertyRendererModel(parent, melding, xPath, prop, values)
    {
        public string Iterator => iteratorElement;
        public XmlSchemaAnnotated? GetIterateTypeDefinition()
        {
            var child = XsdUtils.GetChildByPath(Prop, Iterator);
            if (child != null)
                return XsdUtils.GetSimpleType(child);
            return null;
        }
    }
}
