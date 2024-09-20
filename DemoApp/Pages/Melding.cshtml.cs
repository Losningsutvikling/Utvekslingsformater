using DemoApp.Models;
using DemoApp.Models.Fagsystem;
using DemoApp.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Xml.Schema;

namespace DemoApp.Pages
{
    public class MeldingModel : PageModel
    {
        public void OnGet()
        {
            Init();
        }

        public void OnPost()
        {
            Init();
        }

        public void Init()
        {
            Utils.GetRequestParams(Request, out QueryParams);

            if (QueryParams.ContainsKey("LagHTML"))
            {
                // Lagre til XML
                // Transformere XML v/XSLT
                // Vise melding
            }
            else if (QueryParams.ContainsKey("Send"))
            {
                // Lagre til XML
                // Redirect til 
            }

            string selectedProtocolName = Utils.GetRequestValue(QueryParams, Konstanter.SelectedProtocol);
            if (selectedProtocolName != "")
                SelectedProtocol = XsdUtils.MeldingsprotokollVersjoner.FirstOrDefault(v => v.Version == selectedProtocolName);
            if (SelectedProtocol == null && XsdUtils.MeldingsprotokollVersjoner.Count() == 1)
                SelectedProtocol = XsdUtils.MeldingsprotokollVersjoner.First();
            SelectedAksjon = Utils.GetRequestValue(QueryParams, Konstanter.SelectedAksjon);
            SelectedSkjemaElementName = Utils.GetRequestValue(QueryParams, Konstanter.SelectedSkjema);
            if (SelectedProtocol != null && SelectedSkjemaElementName != "")
                SelectedXmlSchemaElement = XsdUtils.FindByXPath(SelectedProtocol!.TargetNamespace!, SelectedSkjemaElementName);

            ViewData["Title"] = (SelectedSkjemaElementName) ?? "Melding";
            if (!string.IsNullOrEmpty(SelectedSkjemaElementName))
                PrefillValues = TestdataGenerator.GetPrefillValues(Request, SelectedXmlSchemaElement.Name);
        }

        public Dictionary<string, string> QueryParams = [];
        public List<PrefilledValue>? PrefillValues { get; set; } = [];
        public string MeldingId { get; set; } = "";

        public XmlSchemaElement? SelectedXmlSchemaElement { get; set; }
        public XmlSchema? SelectedProtocol { get; set; }
        public string SelectedSkjemaElementName { get; set; } = "";
        public string SelectedAksjon { get; set; } = "";

        public List<ControlEnabler> controlEnablers { get; set; } = [];
    }
}
