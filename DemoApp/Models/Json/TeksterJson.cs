namespace DemoApp.Models.Json
{

    public class TeksterJson
    {
        public List<string> missing = [];
        public List<string> used = [];
        public List<string> wrongName = [];

        public string sprak { get; set; }
        public Henvisning henvisning { get; set; }
    }

    public class Henvisning
    {
        public string versjon { get; set; }
        public string versjonsInformasjon { get; set; }
        public string gjelderFra { get; set; }
        public string[] xsd { get; set; }
        public Skjemaelement[] skjemaelement { get; set; }
    }

    public class Skjemaelement
    {
        public string navn { get; set; }
        public string id { get; set; }
        public string ledetekst { get; set; }
        public string veiledning { get; set; }
        public int sortering { get; set; }
    }

}
