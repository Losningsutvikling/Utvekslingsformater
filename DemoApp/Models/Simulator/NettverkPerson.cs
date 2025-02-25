using System.Text.Json.Serialization;

namespace DemoApp.Models.Simulator
{
    public enum NettverkRelasjonType
    {
        Mor,
        Far,
        Annet
    }
    public class NettverkPerson
    {
        public int Id { get; set; }
        public int Sak_Id { get; set; }
        public int FREG_Person_id { get; set; }

        [JsonIgnore]
        public FREG_Person? fREG_Person { get; set; }
        public NettverkRelasjonType NettverkRelasjon { get; set; }
    }
}
