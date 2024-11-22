using DemoApp.Models;
using DemoApp.Models.Fagsystem;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DemoApp.Pages
{
    public class MeldingLagretModel(IConfiguration config, IWebHostEnvironment env) : PageModel
    {
        public void OnPost()
        {
            Utils.GetRequestParams(Request, out QueryParams);

            if (QueryParams.ContainsKey("Lagre"))
            {

                // Lagre til XML
                // Transformere XML v/XSLT
                // Vise melding
                string configPath = config["TestFilePath"];

                //OBS! Må endres!!!! QueryParams["HenvisningFosterhjem.Meldingshode.Avsender.Organisasjonsnummer"]
                string orgNr = QueryParams["HenvisningFosterhjem.Meldingshode.Avsender.Organisasjonsnummer"];
                string fileName;

                if (QueryParams["selectedAksjon"] == "Oppdatering") {
                    fileName = $"{orgNr}/sendt/oppdatering_{DateTime.Now.ToString("yyyy-dd-MM_HH-mm")}.xml";
                }
                else
                {
                    fileName = $"{orgNr}/sendt/{DateTime.Now.ToString("yyyy-dd-MM_HH-mm")}.xml";
                }

                XmlFactory.CreateXml(config, env, QueryParams, fileName);

                MeldingsHodeListe = XmlFactory.ReadXMlElement(config, orgNr);

            }
            else if (QueryParams.ContainsKey("Send"))
            {

                // Lagre til XML
                // Redirect til 
            }

        }

        public void OnGet()
        {
            OnPost();
        }

        public Dictionary<string, string> QueryParams = [];

        public Dictionary<string, Dictionary<string, string>> MeldingsHodeListe { get; set; } = [];



    }
}
