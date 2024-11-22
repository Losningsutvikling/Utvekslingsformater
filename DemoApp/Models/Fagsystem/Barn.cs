using System.Text.Json.Serialization;

namespace DemoApp.Models.Fagsystem
{

    public enum BarnType
    {
        Standard,
        EMA,
        Ufodt
    }

    public class Barn
    {
        public int Id { get; set; }
        public int _FREG_Person_Id { get; set; }
        [JsonIgnore]
        public FREG_Person? FREG_Person { get; set; }
        public BarnType Type { get; set; } = BarnType.Standard;

        public List<NettverkPerson>? Nettverk { get; set; }

    }
}
