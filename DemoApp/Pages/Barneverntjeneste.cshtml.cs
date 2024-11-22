using DemoApp.Models.Fagsystem;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Xml.Schema;

namespace DemoApp.Pages
{
    public class BarneverntjenesteModel : PageModel
    {
        public void OnGet()
        {
            Init();
        }

        public void OnPost()
        {
            Init();
        }

        public Barneverntjeneste? BVtjeneste { get; set; }

        public XmlSchemaAnnotated? barnevernElement { get; set; }
        public string? path { get; set; }
        public void Init()
        {
            BVtjeneste = TestdataGenerator.GetBarneverntjeneste();
            /*path = "Avsender";
            var schema = DataFactory.XsdSchemasWithRootElement.First()
                ?? throw new Exception($"Finner ikke skjema som inneholder '{path}'");
            var element = schema.Items.OfType<XmlSchemaElement>().First();
            barnevernElement = DataFactory.FindByXPath(element, "Avsender").First();*/
        }
    }
}
