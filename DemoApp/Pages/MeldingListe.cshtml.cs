using DemoApp.Models;
using DemoApp.Models.Simulator;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DemoApp.Pages
{
    public class MeldingListeModel(IConfiguration config/*, IWebHostEnvironment env*/) : PageModel
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
            string selectedSak = Utils.GetRequestValue(QueryParams, Konstanter.SelectedSak);
            string filterFra = $"{Konstanter.MeldingstypeAvsendersRef}={selectedSak}";
            string filterTil = $"{Konstanter.MeldingstypeMottakersRef}={selectedSak}";
            ViewData["Title"] = $"{MeldingSynonym} - liste";
            string folder = XmlFactory.GetMeldingPath(TestdataGenerator.GetBarneverntjeneste().Organisasjonsnummer!, "sendt");
            XmlFactory.ReadFilesMeldingshode(config, folder, meldinger, filterFra);
            folder = XmlFactory.GetMeldingPath(TestdataGenerator.GetBarneverntjeneste().Organisasjonsnummer!, "lagret");
            XmlFactory.ReadFilesMeldingshode(config, folder, meldinger, filterFra);
            folder = XmlFactory.GetMeldingPath(TestdataGenerator.GetBarneverntjeneste().Organisasjonsnummer!, "mottatt");
            XmlFactory.ReadFilesMeldingshode(config, folder, meldinger, filterTil);
            meldinger.Sort((fil1, fil2) => { return DateTime.Compare((DateTime)(fil1.Meldingshode.SendtTidspunkt!), (DateTime)(fil2.Meldingshode.SendtTidspunkt!)); });
        }

        public Dictionary<string, string> QueryParams = [];

        public List<FilInfo> meldinger { get; set; } = [];

        public string MeldingSynonym
        {
            get
            {
                string value = config.GetValue<string>("MeldingSynonym") ?? "Melding";
                return (value != "") ? value : "Melding";
            }
        }
    }
}
