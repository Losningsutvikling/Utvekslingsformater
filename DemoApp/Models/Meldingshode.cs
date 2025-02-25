namespace DemoApp.Models;

public class FilInfo
{
    public string Filnavn { get; set; } = "";
    public Meldingshode Meldingshode { get; set; } = new();
}

public class Meldingshode
{

    public string Id { get; set; } = "";

    public OppfolgingAvMelding? OppfolgingAvMelding { get; set; }

    public string MeldingstypeNmsp { get; set; } = "";

    public string Meldingstype { get; set; } = "";

    public Fagsystem FagsystemAvsender { get; set; } = new();

    public System.DateTime? SendtTidspunkt { get; set; }
    public Organisasjon Avsender { get; set; } = new();
    public Organisasjon Mottaker { get; set; } = new();
    public string AvsendersRef { get; set; } = "";
    public KontaktInfo? KontaktInfoAvsender { get; set; }
    /*    internal void SetValue(string name, string text)
        {
            string[] elementPath = name.Replace(Konstanter.MeldingPrefix, "").Split('.');
            object? propObj = this;
            for (int i = 0; i < elementPath.Length; i++)
            {
                var elementName = elementPath[i];
                var propInfo = propObj!.GetType().GetProperty(elementName);
                if (propInfo != null)
                {
                    if (!propInfo.PropertyType.FullName!.StartsWith("System."))
                    {
                        var childObj = propInfo.GetValue(propObj, null);
                        if (childObj == null)
                        {
                            var newObj = Activator.CreateInstance(propInfo.PropertyType);
                            propInfo.SetValue(propObj, newObj);
                        }
                        propObj = childObj;
                    }
                    else
                    {
                        if (propInfo.PropertyType == typeof(DateTime))
                        {
                            if (DateTime.TryParseExact(text.Trim(), XmlFactory.MELDING_FILNAVN_DATETIME_FORMAT, null, System.Globalization.DateTimeStyles.None, out DateTime dtResult))
                                propInfo.SetValue(propObj, dtResult);
                        }
                        else
                            propInfo.SetValue(propObj, text);
                    }
                }
            }
        }*/

    public string GetValue(string name)
    {
        var propInfo = this.GetType().GetProperty(name);
        if (propInfo != null)
        {
            return propInfo.GetValue(this)?.ToString() ?? "";
        }
        return "";
    }
}

public class OppfolgingAvMelding
{
    public string MeldingsForbindelse { get; set; } = "";
    public string StartMeldingId { get; set; } = "";
    public string MeldingId { get; set; } = "";
}

public class Fagsystem
{
    public string Leverandor { get; set; } = "";
    public string Navn { get; set; } = "";
    public string Versjon { get; set; } = "";
}

public class Organisasjon
{
    public string Organisasjonsnummer { get; set; } = "";
    public string Navn { get; set; } = "";
    public Kommuneinfo? Kommuneinfo { get; set; }
}

public class Kommuneinfo
{
    public string Kommunenummer { get; set; } = "";
    public string Kommunenavn { get; set; } = "";
}

public class KontaktInfo
{
    public Kontaktperson? Kontaktperson { get; set; }
    public Kontaktperson? KontaktpersonLeder { get; set; }
}

public class Kontaktperson
{
    public string Navn { get; set; } = "";
    public string Telefon { get; set; } = "";
    public string epost { get; set; } = "";
}


