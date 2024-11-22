namespace DemoApp.Models.Fagsystem
{
    public class Sak
    {
        public int Id { get; set; }
        public int BarnId { get; set; }
        public Dictionary<string, string> Saksdata = [];
        public List<string> _meldinger { get; set; } = [];
        public List<Melding> Meldinger { get; set; } = [];
    }
}
