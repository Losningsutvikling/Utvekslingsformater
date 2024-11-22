using DemoApp.Models;
using DemoApp.Models.Fagsystem;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DemoApp.Pages
{
    public class BarnModel : PageModel
    {
        public Barn? selectedBarn { get; set; }

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
            var liste = TestdataGenerator.GetTestdata();
            Utils.GetRequestParams(Request, out Dictionary<string, string> queryParams);
            string barnId = Utils.GetRequestValue(queryParams, "barn_id");
            if (!string.IsNullOrEmpty(barnId))
                selectedBarn = liste.FirstOrDefault(b => b.Id.ToString() == barnId);

        }
    }
}
