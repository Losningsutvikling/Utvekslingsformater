using DemoApp.Models.Simulator;
using DemoApp.Pages;
using System.Xml.Schema;
namespace DemoApp.Models.ViewModels
{
    public class PropertyRendererModel(PropertyRendererModel? parent, MeldingModel melding, string parentXPath, XmlSchemaAnnotated prop,
        XmlSchemaAnnotated? skipProp = null, PropertyRendererModel? InnerPropertyRendererModel = null, int? InnerPropertyModelInsertIndex = 0)
    {
        public MeldingModel Melding { get { return melding; } }
        public PropertyRendererModel? ParentModel { get; set; } = parent;
        public string XPath
        {
            get
            {
                string separator = parentXPath.EndsWith(':') ? "" : (parentXPath.EndsWith('.') ? "" : ".");
                return $"{parentXPath}{separator}{XsdUtils.GetName(prop)}";
            }
        }

        public string ParentXPath
        {
            get
            {
                return parentXPath;
            }
        }
        public XmlSchemaAnnotated Prop { get { return prop; } }

        public PropertyRendererModel InnerModel { get { return InnerPropertyRendererModel; } }
        public int? InnerModelIndex { get { return InnerPropertyModelInsertIndex; } }

        public XmlSchemaAnnotated? SkipProp { get; } = skipProp;

        public string? GetCaption(bool fallbackToName)
        {
            string caption = (CustomCaptionPattern != "") ? "" : XsdUtils.GetCaption(Prop, fallbackToName) ?? "";
            return (FilterText != "") ? caption.Replace("{tekst}", FilterText) : caption;
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

        public bool UseCaption { get; set; } = true;
        public bool UseDescription { get; set; } = true;

        public DateTime Start { get; set; } = DateTime.Now.Date;

        //        public string FilterId { get; set; } = "";
        //        public string[] FilterValues { get; set; } = [""];
        public string FilterText { get; set; } = "";
        public string CustomText { get; set; } = "";

        public string CustomCaptionPattern { get; set; } = "";
        public string CustomCaptionStyle { get; set; } = "";

        public string KodelisteFilterVariabel { get; set; } = "";
        public string KodelisteFilterVerdi { get; set; } = "";
        public string GetDescription()
        {
            if (!string.IsNullOrEmpty(CustomText))
                return CustomText;
            if (Prop == null) return
                    "<Prop=null>";
            return XsdUtils.GetDescription(Prop);
        }

        public string GetRawId(string propId = "")
        {
            string id = "";
            if (propId != "")
                id = $"{parentXPath}.{propId}";
            else if (Prop is XmlSchemaElement || Prop is XmlSchemaAttribute || prop is XmlSchemaChoice)
                id = XPath;
            return id.TrimStart('.');
        }
        public string GetId(string propId = "")
        {
            string id = GetRawId(propId);
            if (UseItemCount)
            {
                if (ItemNo > 0)
                    id += $":{ItemNo}";
                else
                    id += ":0.template";
            }
            return id;
        }
        public string GetNameChoice(string propId = "")
        {
            if (Prop is XmlSchemaChoice)
                return $"{XPath}{XsdUtils.GetChoiceElementNames(prop as XmlSchemaChoice)}";
            return "";
        }

        public string GetIdWithItemNo(string itemNo, string propId = "")
        {
            string id = GetRawId(propId);
            id += $":{itemNo}";
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

        public int ItemNo { get; set; } = 0;

        public bool CheckAddEnabler()
        {
            int numPre = Melding.controlEnablers.Count;
            string aktiveringExpression = XsdUtils.GetAppInfoValue(Prop, "enable", false, false);
            if (aktiveringExpression != "")
            {
                string[] parts = aktiveringExpression.Split("=");
                bool isInverted = parts[0].EndsWith('!');
                string controlId = parts[0].TrimEnd('!');
                string aktiveringControlID = GetId(controlId);
                string aktiveringValue = (parts.Length > 0) ? parts[1] : "";
                if (ParentModel?.UseItemCount == true)
                {
                    for (int i = 1; i <= int.Min(XsdUtils.GetMaxOccurs(ParentModel.Prop), 10); i++)
                    {
                        string id = GetId("").Replace(":0.template", $":{i}");
                        Melding.controlEnablers.Add(new(id, GetId(controlId).Replace(":0.template", $":{i}"), aktiveringValue, isInverted));
                    }
                }
                else
                {
                    Melding.controlEnablers.Add(new(GetId(), aktiveringControlID, aktiveringValue, isInverted));
                }
            }
            return Melding.controlEnablers.Count > numPre;
        }

        public List<List<PrefilledValue>> GetPrefilledItems(string xPath)
        {
            return Melding.GetPrefilledValuesMultiple(xPath.Replace(":0.template", ":"));
        }

    }
}
