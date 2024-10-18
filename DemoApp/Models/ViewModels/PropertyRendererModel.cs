using DemoApp.Models.Fagsystem;
using DemoApp.Pages;
using System.Xml.Schema;
namespace DemoApp.Models.ViewModels
{
    public class PropertyRendererModel(PropertyRendererModel? parent, MeldingModel melding, string parentXPath, XmlSchemaAnnotated prop, List<PrefilledValue>? values, XmlSchemaAnnotated? skipProp = null)
    {
        public MeldingModel Melding { get { return melding; } }
        public PropertyRendererModel? ParentModel { get; set; } = parent;
        public string XPath { get; } = $"{parentXPath}.{XsdUtils.GetName(prop)}";
        public XmlSchemaAnnotated Prop { get { return prop; } }
        public List<PrefilledValue>? Values { get; } = values;

        public XmlSchemaAnnotated? SkipProp { get; } = skipProp;

        public string? GetCaption(bool fallbackToName)
        {
            return XsdUtils.GetCaption(Prop, fallbackToName);
        }

        public bool UseItemCount
        {
            get
            {
                return XsdUtils.GetIsRepeating(prop);
            }
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
        }

        public string GetRawId(string propId = "")
        {
            string id = "";
            if (propId != "")
                id = $"{parentXPath}.{propId}";
            else if (Prop is XmlSchemaElement || Prop is XmlSchemaAttribute)
                id = XPath;
            return id.TrimStart('.');
        }
        public string GetId(string propId = "")
        {
            string id = GetRawId(propId);
            if (UseItemCount)
                id += ":0:template";
            return id;
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
            string aktiveringExpression = XsdUtils.GetAppInfoValue(Prop, "enable", false);
            if (aktiveringExpression != "")
            {
                string[] parts = aktiveringExpression.Split("=");
                string aktiveringControlID = GetId(parts[0]);
                string aktiveringValue = (parts.Length > 0) ? parts[1] : "";
                if (ParentModel?.UseItemCount == true)
                {
                    for (int i = 1; i <= int.Min(XsdUtils.GetMaxOccurs(ParentModel.Prop), 10); i++)
                    {
                        string id = GetId("").Replace(":0:template", $":{i}");
                        Melding.controlEnablers.Add(new(id, GetId(parts[0]).Replace(":0:template", $":{i}"), aktiveringValue));
                    }
                }
                else
                {
                    Melding.controlEnablers.Add(new(GetId(), aktiveringControlID, aktiveringValue));
                }
            }
        }

        public XmlSchemaSimpleType? GetIterateTypeDefinition()
        {
            var constraint = XsdUtils.GetUniqueConstraint(prop);
            if (constraint != null)
            {
                var iteratorProp = constraint.Selector!.XPath
                    ?? throw new Exception($"Ingen selector i unique constraint i element {XsdUtils.GetName(prop)}");
                var enumProp = (iteratorProp == ".") ? prop : XsdUtils.GetChildByPath(prop, iteratorProp);
                if (enumProp != null)
                    return XsdUtils.GetSimpleType(enumProp);
            }
            return null;
        }

    }
}
