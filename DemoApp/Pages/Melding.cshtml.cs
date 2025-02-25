using DemoApp.Models;
using DemoApp.Models.Simulator;
using DemoApp.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Xml.Schema;

namespace DemoApp.Pages
{
    public class MeldingModel(IConfiguration config/*, IWebHostEnvironment env*/) : PageModel
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
            ExistingFile = Utils.GetRequestValue(QueryParams, @Konstanter.XmlFil);
            bool isSent = false;
            if (ExistingFile != "")
            {
                isSent = ExistingFile.Contains("/sendt/");
                var filinfo = XmlFactory.ReadFileMeldingshode(config, ExistingFile);
                var existingTargetNmsp = filinfo.Meldingshode.MeldingstypeNmsp;
                string SelectedNamespace = Utils.GetRequestValue(QueryParams, Konstanter.SelectedSkjema);
                bool changeMeldingstype = SelectedNamespace != "" && SelectedNamespace != existingTargetNmsp;
                if (changeMeldingstype) // Endre skjema på eksisterende fil!
                {
                    existingTargetNmsp = SelectedNamespace;
                }

                SelectedSchema = XsdUtils.SchemaFromTargetNmsp(existingTargetNmsp)
                    ?? throw new Exception($"Schema ikke funnet for {existingTargetNmsp}");

                XmlSchemaElement rootElement = XsdUtils.GetRootElement(SelectedSchema)
                    ?? throw new Exception($"Schema {SelectedSchema!.TargetNamespace} mangler rotelement");
                if (changeMeldingstype)
                {
                    QueryParams[$"{rootElement.Name}.{Konstanter.Meldingshode}.{Konstanter.MeldingstypeNmsp}"] = SelectedNamespace;
                    SelectedSchema = XsdUtils.SchemaFromTargetNmsp(SelectedNamespace);
                    rootElement = XsdUtils.GetRootElement(SelectedSchema)
                        ?? throw new Exception($"Schema {SelectedSchema!.TargetNamespace} mangler rotelement");
                }
                SelectedProtocol = XsdUtils.SchemaProtocolFromSchema(SelectedSchema!);
                SelectedProtocolDisabled = true;
                var selectedSchemaElement = XsdUtils.GetRootElement(SelectedSchema);

                XmlFactory.ReadXMLFile(SelectedSchema, config, ExistingFile, true);

                PrefillValues = XmlFactory.ReadXMLFile(SelectedSchema, config, ExistingFile, true);
                if (changeMeldingstype)
                {
                    string caption = XsdUtils.GetCaption(rootElement, true);
                    filinfo.Meldingshode.MeldingstypeNmsp = existingTargetNmsp;
                    filinfo.Meldingshode.Meldingstype = caption;
                }
                SelectedAksjon = (isSent) ? "Oppdatering" : "Ny";
                SelectedAksjonDisabled = true;
                if ((Utils.GetRequestValue(QueryParams, "actionParam") == "EndreType") || changeMeldingstype)
                {
                    if (XsdUtils.MeldingsprotokollVersjoner.Count() == 1)
                        SelectedProtocol = XsdUtils.MeldingsprotokollVersjoner.First()
                            ?? throw new NotImplementedException("Håndterer ikke oppsett med flere meldingsprotokollversjoner!");

                    AvailableSkjema = XsdUtils.XsdSchemaNamesWithNamedRootElement(rootElement.Name ?? "-");
                }
                else
                {
                    SelectedSkjemaDisabled = true;
                }
                if (isSent)
                {
                    TestdataGenerator.Update_MeldingsInfo(PrefillValues, SelectedAksjon, SelectedSchema, filinfo);
                }
                TestdataGenerator.UpdateEditableAndClosed(PrefillValues, rootElement.Name!);

                ViewData["Title"] = $"{((isSent) ? SelectedAksjon : "Videre redigering")} av melding";
            }
            else
            {
                string selectedProtocolName = Utils.GetRequestValue(QueryParams, Konstanter.SelectedProtocol);
                if (selectedProtocolName != "")
                    SelectedProtocol = XsdUtils.MeldingsprotokollVersjoner.FirstOrDefault(v => v.Version == selectedProtocolName);
                if (SelectedProtocol == null && XsdUtils.MeldingsprotokollVersjoner.Count() == 1)
                    SelectedProtocol = XsdUtils.MeldingsprotokollVersjoner.First();
                SelectedAksjon = Utils.GetRequestValue(QueryParams, Konstanter.SelectedAksjon);
                if (SelectedAksjon == "")
                    SelectedAksjon = "Ny";
                SelectedSchema = XsdUtils.SchemaFromTargetNmsp(Utils.GetRequestValue(QueryParams, Konstanter.SelectedSkjema));
                var rootElement = XsdUtils.GetRootElement(SelectedSchema);
                var rootElementName = rootElement?.Name ?? "";
                string? caption = XsdUtils.GetCaption(rootElement, true) ?? "Melding";

                if ((rootElement != null) && (SelectedAksjon == "Ny"))
                    PrefillValues = TestdataGenerator.GetPrefillValues(Request, null);
                ViewData["Title"] = (rootElementName) ?? "Melding";
            }

        }

        public string ExistingFile { get; set; } = "";

        public Dictionary<string, string> QueryParams = [];

        public List<PrefilledValue>? PrefillValues { get; set; } = [];
        public string MeldingId { get; set; } = "";

        public XmlSchema? SelectedProtocol { get; set; }
        public bool SelectedProtocolDisabled { get; private set; } = false;
        public XmlSchema? SelectedSchema { get; set; } = null;
        public bool SelectedSkjemaDisabled { get; set; } = false;

        public string SelectedAksjon { get; set; } = "";
        public bool SelectedAksjonDisabled { get; private set; } = false;
        public string[] AvailableSkjema { get; private set; } = [];
        public bool TestTekster { get; set; } = false;

        public List<ControlEnabler> controlEnablers { get; set; } = [];

        public PrefilledValue? GetPrefilledValueFor(string id)
        {
            var hit = PrefillValues?.FirstOrDefault(p => id.EndsWith(p.Xpath));
            return hit;
        }

        public List<List<PrefilledValue>> GetPrefilledValuesMultiple(string id)
        {
            List<List<PrefilledValue>> result = [];
            int itemNo = 1;
            while (true && itemNo < 100)
            {
                var items = PrefillValues?.Where(p => p.Xpath == $"{id}{itemNo}" || p.Xpath.StartsWith($"{id}{itemNo}.")).ToList();
                if (items?.Count > 0)
                    result.Add(items);
                else
                    break;
                itemNo++;
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

        /*        public KodelisteFilter? kodelistefiltre { get; set; }

                public List<Kode> FilterKodeliste(Kodeliste liste, string filterliste_id = "", string[]? filterverdier = null)
                {
                    List<Kode> result = [];
                    if (filterliste_id != "" && filterverdier?.Length > 0)
                    {
                        result = XsdUtils.GetFilteredKodeliste(liste, filterliste_id, filterverdier);
                    }
                    else if (kodelistefiltre != null && kodelistefiltre.filter.Any(f => f.kodeliste_id == liste.id))
                    {
                        var filter = kodelistefiltre.filter.FirstOrDefault(f => f.kodeliste_id == liste.id);
                        result = XsdUtils.GetFilteredKodeliste(liste, filter.filterliste_id, filter.filterverdier);
                    }
                    else
                        result = liste.koder.ToList();
                    return result;

                }
        */
    }
}
