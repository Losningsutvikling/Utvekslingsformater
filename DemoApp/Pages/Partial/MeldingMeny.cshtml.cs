using DemoApp.Models;

namespace DemoApp.Pages.Partial
{
    /*  Fra MeldingsAksjon.cshtml:
        var aksjonEnum = XsdUtils.GetTypeDefinition(Model.SelectedProtocol, Konstanter.MeldingsForbindelseType) ??
            throw new Exception($"Finner ikke aksjonsverdier for type {Konstanter.MeldingsForbindelseType}");
        var kodeliste = XsdUtils.GetKodelisteVerdier(aksjonEnum);
    */
    public class MeldingMenyModel
    {
        public MeldingMenyModel(FilInfo fileInfo)
        {
            string status = XmlFactory.GetStatusFromFile(fileInfo);
            string id = fileInfo.Meldingshode.Id;
            if (status == "Lagret")
            {
                actions.Add(new("Redigér videre", $"menuActionMeldingListe(event, 'Melding')"));
                actions.Add(new("Redigér som annen type", $"menuActionMeldingListe(event, 'Melding', 'actionParam=EndreType')"));
                actions.Add(new("Send", $"menuActionMeldingListe(event, 'LagreMelding', 'actionParam=sendt')"));
                actions.Add(new("Slett (lokalt)", $"menuActionMeldingListe(event, 'SlettMelding')"));
            }
            else if (status == "Sendt")
            {
                actions.Add(new("Oppdatér/Redigér", $"menuActionMeldingListe(event, 'Melding', 'actionParam=Oppdatering')"));
                actions.Add(new("Oppdatér/Redigér som annen type", $"menuActionMeldingListe(event, 'Melding', 'actionParam=EndreType')"));
                actions.Add(new("Trekk tilbake / slett", $"menuActionMeldingListe(event, 'TrekkMelding')"));
            }
            else if (status == "Mottatt")
            {
                actions.Add(new("Svar", $"menuActionMeldingListe(event, 'MeldingXX')"));
            }
            actions.Add(new("Info", $"menuActionMeldingListe(event, 'MeldingInfo');"));
        }

        public List<KeyValuePair<string, string>> actions { get; set; } = [];

    }
}
