using DemoApp.Models.Fagsystem;
using DemoApp.Pages;
using System.Xml.Schema;
namespace DemoApp.Models.ViewModels
{
    public class PropertyRendererModel(MeldingModel melding, string parentXPath, XmlSchemaAnnotated prop, List<PrefilledValue>? values, XmlSchemaAnnotated? skipProp = null)
    {
        public MeldingModel Melding { get; set; } = melding;
        public string XPath { get; } = $"{parentXPath}/{XsdUtils.GetName(prop)}";
        public XmlSchemaAnnotated Prop { get; } = prop;
        public List<PrefilledValue>? Values { get; } = values;

        public XmlSchemaAnnotated? SkipProp { get; } = skipProp;

        public string? GetCaption(bool fallbackToName)
        {

            return XsdUtils.GetCaption(Prop, fallbackToName);
        }

        public bool Mandatory
        {
            get
            {
                return XsdUtils.PropIsMandatory(Prop);
            }
        }

        public DateTime Start { get; set; } = DateTime.Now.Date;

        public string GetDescription()
        {
            if (Prop == null) return
                    "<Prop=null>";
            return XsdUtils.GetDescription(Prop);
            return XsdUtils.GetAppInfoValue(Prop, "veiledning");
        }

        public string GetId(string propId = "")
        {
            if (propId != "")
                return $"{parentXPath}/{propId}".Replace('/', '_');
            if (Prop is XmlSchemaElement || Prop is XmlSchemaAttribute)
                return XPath.Replace('/', '_');
            return $"{(Prop.Id ?? Prop.ToString())}---{XPath.Replace('/', '_')}";
        }

        public int GetMinLength()
        {
            return XsdUtils.GetMinLength(Prop);
        }
        public int GetMaxLength()
        {
            return XsdUtils.GetMaxLength(Prop);
        }

        public XmlSchemaAnnotated? GetChildTypeDefinition(string path)
        {
            var child = XsdUtils.GetChildByPath(Prop, path);
            if (child != null)
                return XsdUtils.GetSimpleType(child);
            return null;
        }

        public void CheckAddEnabler()
        {
            string aktiveringExpression = XsdUtils.GetAppInfoValue(Prop, "aktivering", false);
            if (aktiveringExpression != "")
            {
                string[] parts = aktiveringExpression.Split("=");
                string aktiveringControlID = GetId(parts[0]);
                string aktiveringValue = (parts.Length > 0) ? parts[1] : "";
                Melding.controlEnablers.Add(new(GetId(), aktiveringControlID, aktiveringValue));
            }
        }
    }
}
