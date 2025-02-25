using DemoApp.Models;
using DemoApp.Models.Test;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DemoApp.Pages
{
    public class TestSchemaModel : PageModel
    {
        public TestSchemaModel()
        {
        }

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
            Kodelistefeil = XsdTester.TestKodelister();
            Tekstfeil = XsdTester.TestJsonTekster();
            Idfeil = XsdTester.TestIder();
        }
        public List<string> GetNonUsed()
        {
            List<string> result = [];
            foreach (var item in XsdUtils.Tekster.henvisning.skjemaelement)
            {
                string itemValue = $"{item.id} - {item.navn}";
                if (XsdUtils.Tekster.used.IndexOf(itemValue) < 0)
                    result.Add(itemValue);
            }
            return result;
        }

        public List<string> Kodelistefeil { get; set; } = [];
        public List<string> Tekstfeil { get; set; } = [];
        public List<string> Idfeil { get; set; } = [];


    }
}
