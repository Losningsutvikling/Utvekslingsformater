namespace DemoApp.Models.Simulator
{

    public enum AdresseGradering
    {
        ugradert,
        fortrolig,
        strengtFortrolig
    }

    public class FREG_Person
    {
        public int Id { get; set; }
        public AdresseGradering Graderingsnivaa { get; set; } = AdresseGradering.ugradert;
        public string? Fodselsnummer { get; set; }
        public string? Fornavn { get; set; }
        public string? Etternavn { get; set; }

        public string? Kjonn { get; set; }
        public DateTime Fodselsdato { get; set; }


    }
}
