using DemoApp.Models;
using DemoApp.Models.Fagsystem;
using DemoApp.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Xml.Schema;

namespace DemoApp.Pages
{
    public class MeldingModel(IConfiguration config, IWebHostEnvironment env) : PageModel
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
                if (SelectedAksjon == "Ny")
                    PrefillValues = TestdataGenerator.GetPrefillValues(Request, SelectedXmlSchemaElement.Name);
                else if (SelectedAksjon == "Oppdatering")
                {
                    //var filename = "testfil_fosterhjem_henvisning.xml";
                    var filename = "974635453/sendt/2024-20-11_12-09.xml"; // OBS endre fra hardkodet til variabel 
                    PrefillValues = XmlFactory.ReadXMLFile(QueryParams, config, filename, "HenvisningFosterhjem", true);
                }
            InitKodelisteFiltre();
            XsdUtils.Tekster.missing.Clear();
            XsdUtils.Tekster.used.Clear();
            XsdUtils.Tekster.wrongName.Clear();
        }
        private void InitKodelisteFiltre()
        {
            if (SelectedXmlSchemaElement != null)
            {
                string? kodelistefilterXML = XsdUtils.GetAppInfoValue(SelectedXmlSchemaElement, "kodelistefilter", true, false);
                if (!string.IsNullOrEmpty(kodelistefilterXML))
                {
                    kodelistefiltre = Hack.XmlToKodelistefilter(kodelistefilterXML);
                    /*var serializer = new XmlSerializer(typeof(KodelisteFilter));
                    XmlReader xReader = XmlReader.Create(new StringReader(kodelistefilterXML));
                    serializer.Deserialize(xReader) as KodelisteFilter;*/
                }
            }
        }

        public Dictionary<string, string> QueryParams = [];

        public List<PrefilledValue>? PrefillValues { get; set; } = [];
        public string MeldingId { get; set; } = "";

        public XmlSchemaElement? SelectedXmlSchemaElement { get; set; }
        public XmlSchema? SelectedProtocol { get; set; }
        public string SelectedSkjemaElementName { get; set; } = "";
        public string SelectedAksjon { get; set; } = "";

        public bool TestTekster { get; set; } = false;

        public List<ControlEnabler> controlEnablers { get; set; } = [];

        public PrefilledValue? GetPrefilledValueFor(string id)
        {
            var hit = PrefillValues?.FirstOrDefault(p => id.EndsWith(p.Xpath));
            return hit;
        }


        public List<string> GetNonUsed()
        {
            List<string> result = [];
            if (SelectedXmlSchemaElement != null) // Vent til det er et ordentlig skjema :-)
            {
                foreach (var item in XsdUtils.Tekster.henvisning.skjemaelement)
                {
                    string itemValue = $"{item.id} - {item.navn}";
                    if (XsdUtils.Tekster.used.IndexOf(itemValue) < 0)
                        result.Add(itemValue);
                }
            }
            return result;
        }

        public string MeldingSynonym
        {
            get
            {
                string value = config.GetValue<string>("MeldingSynonym") ?? "Melding";
                return (value != "") ? value : "Melding";
            }
        }

        public KodelisteFilter? kodelistefiltre { get; set; }

        public List<Kode> FilterKodeliste(Kodeliste liste, string filterliste_id = "", string filterverdi = "")
        {
            List<Kode> result = [];
            if (filterliste_id != "" && filterverdi != "")
            {
                result = XsdUtils.GetFilteredKodeliste(liste, filterliste_id, filterverdi);
            }
            else if (kodelistefiltre != null && kodelistefiltre.filter.Any(f => f.kodeliste_id == liste.id))
            {
                var filter = kodelistefiltre.filter.FirstOrDefault(f => f.kodeliste_id == liste.id);
                result = XsdUtils.GetFilteredKodeliste(liste, filter.filterliste_id, filter.filterverdi);
            }
            else
                result = liste.koder.ToList();
            return result;

        }

    }
}
