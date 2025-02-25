namespace DemoApp.Models;

public class Vedlegg
{
    public string Id { get; set; }
    public string Filnavn { get; set; }
    public string VedleggType { get; set; }
    public string Beskrivelse { get; set; } = "";
    public object? Contents { get; set; }

}
