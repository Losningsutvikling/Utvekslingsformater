using DemoApp.Models;
using DemoApp.Models.Simulator;

namespace DemoApp.Pages.Partial
{
    public class BarnSelectorModel
    {
        public BarnSelectorModel(HttpRequest request) : base()
        {
            Init(request);
        }

        public List<Barn>? BarnListe { get; private set; }

        public Barn? selectedBarn { get; set; }

        public void Init(HttpRequest request)
        {
            this.BarnListe = TestdataGenerator.GetTestdata();
            Utils.GetRequestParams(request, out Dictionary<string, string> queryParams);
            string barnId = Utils.GetRequestValue(queryParams, Konstanter.SelectedSak);
            if (!string.IsNullOrEmpty(barnId))
                selectedBarn = BarnListe.FirstOrDefault(b => b.Id.ToString() == barnId);

        }
    }
}
