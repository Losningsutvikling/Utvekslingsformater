using DemoApp.Pages.Partial;
using System.Xml.Schema;

namespace DemoApp.Models.Simulator
{
    public static class TestdataGenerator
    {

        private static readonly List<Barn> testbarn = GenerateTestdata();

        public static List<Barn> GetTestdata()
        {
            return testbarn;
        }

        private static List<FREG_Person> addFregData()
        {
            List<FREG_Person> personer =
            [
                new()
                {
                    Id = 10000001,
                    Fornavn = "Bjarne",
                    Etternavn = "Wern",
                    Fodselsdato = new DateTime(2012, 12, 12),
                    Fodselsnummer = "12121241555",
                    Graderingsnivaa = AdresseGradering.ugradert,
                    Kjonn = "mann"
                },
                new()
                {
                    Id = 10000002,
                    Fornavn = "Birgitte",
                    Etternavn = "Wern",
                    Fodselsdato = new DateTime(2010, 10, 10),
                    Fodselsnummer = "10101041444",
                    Graderingsnivaa = AdresseGradering.ugradert,
                    Kjonn = "kvinne"
                },
                new()
                {
                    Id = 10000003,
                    Fornavn = "Rigmor",
                    Etternavn = "Wern",
                    Fodselsdato = new DateTime(1990, 10, 10),
                    Fodselsnummer = "10109051455",
                    Graderingsnivaa = AdresseGradering.ugradert,
                    Kjonn = "kvinne"
                },
                new()
                {
                    Id = 10000004,
                    Fornavn = "Rigfar",
                    Etternavn = "Hermann",
                    Fodselsdato = new DateTime(1990, 12, 12),
                    Fodselsnummer = "12129051555",
                    Graderingsnivaa = AdresseGradering.ugradert,
                    Kjonn = "mann"
                },
                new()
                {
                    Id = 10000005,
                    Fornavn = "Alexander",
                    Etternavn = "Hermann",
                    Fodselsdato = new DateTime(1949, 02, 02),
                    Fodselsnummer = "02024953555",
                    Graderingsnivaa = AdresseGradering.ugradert,
                    Kjonn = "mann"
                }
                ];
            return personer;
        }

        private static List<Barn> addBarn()
        {
            List<Barn> barn =
            [
                new Barn()
                {
                    Id = 100010001,
                    _FREG_Person_Id = 10000001,
                    Type = BarnType.Standard,
                    Nettverk = [
                        new NettverkPerson()
                        {
                            Id = 12000001,
                            Sak_Id = 100010001,
                            NettverkRelasjon = NettverkRelasjonType.Mor,
                            FREG_Person_id = 10000003
                        },
                        new NettverkPerson()
                        {
                            Id = 12000002,
                            Sak_Id = 100010001,
                            NettverkRelasjon = NettverkRelasjonType.Far,
                            FREG_Person_id = 10000004
                        },
                        new NettverkPerson()
                        {
                            Id = 12000003,
                            Sak_Id = 100010001,
                            NettverkRelasjon = NettverkRelasjonType.Annet,
                            FREG_Person_id = 10000002
                        },
                        new NettverkPerson()
                        {
                            Id = 12000004,
                            Sak_Id = 100010001,
                            NettverkRelasjon = NettverkRelasjonType.Annet,
                            FREG_Person_id = 10000005
                        }
                    ]
                },
                new Barn()
                {
                    Id = 100010002,
                    _FREG_Person_Id = 10000003,
                    Type = BarnType.Ufodt,
                    Nettverk = [
                        new NettverkPerson()
                        {
                            Id = 12000005,
                            Sak_Id = 100010002,
                            NettverkRelasjon = NettverkRelasjonType.Mor,
                            FREG_Person_id = 10000003
                        },
                        new NettverkPerson()
                        {
                            Id = 12000002,
                            Sak_Id = 100010002,
                            NettverkRelasjon = NettverkRelasjonType.Far,
                            FREG_Person_id = 10000004
                        }
                    ]
                }
            ];
            return barn;
        }

        private static List<Sak> addSaksdata()
        {
            List<Sak> saker =
                [
                    new Sak() // Bjarne Wern
                    {
                        Id = 10000001,
                        BarnId = 10000001,
                        Saksdata = new Dictionary<string, string>()
                        {
                            { "FellesInfo.BarnMedNettverk.BarnetsSituasjon.Omrade", "Omrade=1|Beskrivelse=" },
                        }
                    },

                    new Sak() // Birgitte Wern
                    {
                        Id = 10000002,
                        BarnId = 10000002,
                    }
                    ];
            return saker;
        }

        private static void initBarn(List<Barn> barn, List<FREG_Person> freg)
        {
            foreach (Barn b in barn)
            {
                b.FREG_Person = freg.FirstOrDefault(f => f.Id == b._FREG_Person_Id);
                foreach (NettverkPerson np in b.Nettverk ?? [])
                {
                    np.fREG_Person = freg.FirstOrDefault(f => f.Id == np.FREG_Person_id);
                }
            }
        }

        public static List<Barn> GenerateTestdata()
        {
            var freg = addFregData();
            var barn = addBarn();
            initBarn(barn, freg);
            return barn;
        }

        public static Barneverntjeneste GetBarneverntjeneste()
        {
            return new Barneverntjeneste()
            {
                Navn = "Barneverntjenesten i Asker",
                Organisasjonsnummer = "974635453",
                Kommunenummer = "3203",
                KommuneNavn = "Asker",
                Bydelsnavn = "",
                Bydelsnummer = ""
            };
        }

        public static void GetPrefillValues_Avsender(HttpRequest request, string rotElement)
        {

        }
        public static void GetPrefillValues_Klient(HttpRequest request, string rotElement)
        {

        }

        private static void UpdateValue(List<PrefilledValue> values, string element, string value, bool addIfNonExisting)
        {
            var item = values.FirstOrDefault(pfv => pfv.Xpath == element);
            if (item == null)
            {
                if (addIfNonExisting)
                {
                    item = new(element, value, true, false);
                    values.Add(item);
                }
            }
            else
                item.Value = value;
        }

        public static DateTime Update_SendtTidspunkt(Dictionary<string, string> values, string rootElementName)
        {
            DateTime value = DateTime.Now;
            values[$"{rootElementName}.Meldingshode.SendtTidspunkt"] = value.ToString(XsdUtils.DateTimeFormatXML);
            return value;
        }

        public static void AddOrUpdate(this List<PrefilledValue> list, PrefilledValue rec)
        {
            var existing = list.FirstOrDefault(pfl => pfl.Xpath == rec.Xpath);
            if (existing != null)
            {
                existing.Value = rec.Value;
                existing.InitiallyClosed = rec.InitiallyClosed;
                existing.OpenToEdit = rec.OpenToEdit;
            }
            else
                list.Add(rec);
        }

        public static void Update_MeldingsInfo(List<PrefilledValue> values, string aksjon, XmlSchema schema, FilInfo? existingFilinfo)
        {
            XmlSchemaElement rootElement = XsdUtils.GetRootElement(schema)
                ?? throw new Exception($"Ingen rotelement i schema '{schema.TargetNamespace}'");
            string caption = XsdUtils.GetCaption(rootElement, true);
            string rootElementName = rootElement.Name!;
            if (aksjon != "")
            {
                if (aksjon != "Ny")
                {
                    var aksjonEnum = XsdUtils.GetTypeDefinition(XsdUtils.GetProtocolSchema(schema), Konstanter.MeldingsForbindelseType) ??
                        throw new Exception($"Finner ikke aksjonsverdier for type {Konstanter.MeldingsForbindelseType}");
                    var kodeliste = XsdUtils.GetKodeliste(aksjonEnum) ??
                        throw new Exception($"Finner ikke kodeliste for type {XsdUtils.GetName(aksjonEnum)}");
                    if (kodeliste.koder == null)
                        throw new Exception($"Finner ikke koder i kodeliste {XsdUtils.GetName(aksjonEnum)}");
                    var kode = kodeliste.koder.FirstOrDefault(kvp => kvp.verdi == aksjon)
                        ?? throw new Exception($"Meldingsforbindelse '{aksjon}' finnes ikke i kodeliste");
                    var varRetning = kode.variabler.First(v => v.navn == Konstanter.MeldingsForbindelseType_Variabel_Retning)
                        ?? throw new Exception($"Variabel '{Konstanter.MeldingsForbindelseType_Variabel_Retning}' ikke funnet i kodeliste '{Konstanter.MeldingsForbindelseType}'");
                    values.AddOrUpdate(new($"{rootElementName}.Meldingshode.OppfolgingAvMelding.MeldingsForbindelse", aksjon, false));
                    if (existingFilinfo != null)
                    {
                        if (existingFilinfo.Meldingshode.Id == "")
                            throw new Exception("Finner ikke 'Id' i meldingshode");
                        string startId = existingFilinfo.Meldingshode.Id ?? "";
                        if ((existingFilinfo.Meldingshode.OppfolgingAvMelding?.StartMeldingId ?? "") != "")
                        {
                            startId = existingFilinfo.Meldingshode.OppfolgingAvMelding?.StartMeldingId ?? "";
                            string gjelderId = existingFilinfo.Meldingshode.OppfolgingAvMelding?.MeldingId ?? "";
                            values.AddOrUpdate(new($"{rootElementName}.Meldingshode.OppfolgingAvMelding.StartMeldingId", startId, false));
                            values.AddOrUpdate(new($"{rootElementName}.Meldingshode.OppfolgingAvMelding.MeldingId", gjelderId, false));
                        }
                        if (varRetning.verdi == "mottatt")
                        {
                            if (existingFilinfo.Meldingshode.AvsendersRef != "")
                                // melding er oppfølging av tidligere melding sendt av OSS
                                values.AddOrUpdate(new($"{rootElementName}.Meldingshode.OppfolgingAvMelding.MottakersRef", existingFilinfo.Meldingshode.AvsendersRef, false));
                        }
                    }
                }

            }
            values.AddOrUpdate(new($"{rootElementName}.Meldingshode.MeldingstypeNmsp", schema.TargetNamespace!, false, false));
            values.AddOrUpdate(new($"{rootElementName}.Meldingshode.Meldingstype", caption, false, false));
            values.AddOrUpdate(new($"{rootElementName}.Meldingshode.Id", Guid.NewGuid().ToString(), false));
        }

        private static void SetDisabledAndClosedElements(List<PrefilledValue> values, string path, bool openToEdit)
        {
            var affectedItems = values.Where(pfv => pfv.Xpath.StartsWith(path)).ToList();
            foreach (var item in affectedItems)
            {
                item.OpenToEdit = openToEdit;
            }
        }

        public static void UpdateEditableAndClosed(List<PrefilledValue> values, string rootElement)
        {
            SetDisabledAndClosedElements(values, $"{rootElement}.Meldingshode", false);
            var Meldingshode = values.FirstOrDefault(pfv => pfv.Xpath == $"{rootElement}.Meldingshode");
            if (Meldingshode == null)
                values.Add(new($"{rootElement}.Meldingshode", "", false, true));
            else
                Meldingshode.InitiallyClosed = true;
            SetDisabledAndClosedElements(values, $"{rootElement}.Meldingshode.KontaktInfoAvsender", true);
            SetDisabledAndClosedElements(values, $"{rootElement}.Klient.Identifikator", false);
            SetDisabledAndClosedElements(values, $"{rootElement}.Klient.KommunalSaksId", false);
        }

        public static List<PrefilledValue> GetPrefillValues(HttpRequest request, FilInfo? existingMeldingshode)
        {
            List<PrefilledValue> result = [];

            var selectedSchema = Utils.GetRequestValue(request, Konstanter.SelectedSkjema) ?? "";
            var schema = XsdUtils.GetSchema(selectedSchema);
            var rootElement = XsdUtils.GetRootElement(schema)
                ?? throw new Exception($"Schema {schema.TargetNamespace} inneholder ikke noe rotelement");
            var rootElementName = rootElement.Name;
            var caption = XsdUtils.GetCaption(rootElement, true);

            var selectedAksjon = Utils.GetRequestValue(request, Konstanter.SelectedAksjon) ?? "Ny";

            Update_MeldingsInfo(result, selectedAksjon, schema, existingMeldingshode);

            BarnSelectorModel model = new(request);
            var barn = model.selectedBarn;

            result.Add(new($"{rootElementName}.Meldingshode", "", false, true));
            result.Add(new($"{rootElementName}.Meldingshode.MeldingstypeNmsp", schema.TargetNamespace!, false));
            result.Add(new($"{rootElementName}.Meldingshode.SendtTidspunkt", "", false)); // for at elementet skal være der
            result.Add(new($"{rootElementName}.Meldingshode.Meldingstype", caption, false));
            result.Add(new($"{rootElementName}.Meldingshode.AvsendersRef", barn?.Id.ToString() ?? "", false));
            result.Add(new($"{rootElementName}.Meldingshode.OppfolgingAvMelding", "", false));

            result.Add(new($"{rootElementName}.Meldingshode.FagsystemAvsender.Leverandor", "Visma", false));
            result.Add(new($"{rootElementName}.Meldingshode.FagsystemAvsender.Navn", "Visma Flyt Barnevern", false));
            result.Add(new($"{rootElementName}.Meldingshode.FagsystemAvsender.Versjon", "2.2.234", false));

            var bvTjeneste = GetBarneverntjeneste();
            result.Add(new($"{rootElementName}.Meldingshode.Avsender.Organisasjonsnummer", bvTjeneste?.Organisasjonsnummer ?? "---", false));
            result.Add(new($"{rootElementName}.Meldingshode.Avsender.Navn", bvTjeneste?.Navn ?? "---", false));
            result.Add(new($"{rootElementName}.Meldingshode.Avsender.Kommuneinfo.Kommunenummer", bvTjeneste?.Kommunenummer ?? "---", false));
            result.Add(new($"{rootElementName}.Meldingshode.Avsender.Kommuneinfo.Kommunenavn", bvTjeneste?.KommuneNavn ?? "---", false));
            result.Add(new($"{rootElementName}.Meldingshode.Avsender.Kommuneinfo.Bydelsinfo", false.ToString(), false));
            result.Add(new($"{rootElementName}.Meldingshode.Avsender.Kommuneinfo.Bydelsinfo.Bydelsnummer", bvTjeneste?.Bydelsnummer ?? "---", false));
            result.Add(new($"{rootElementName}.Meldingshode.Avsender.Kommuneinfo.Bydelsinfo.Bydelsnavn", bvTjeneste?.Bydelsnavn ?? "---", false));

            result.Add(new($"{rootElementName}.Meldingshode.Mottaker.Organisasjonsnummer", "986128433", false));
            result.Add(new($"{rootElementName}.Meldingshode.Mottaker.Navn", "Barne- ungdoms- og familieetaten", false));
            result.Add(new($"{rootElementName}.Meldingshode.Mottaker.Kommuneinfo.Kommunenummer", "", false));
            result.Add(new($"{rootElementName}.Meldingshode.Mottaker.Kommuneinfo.Kommunenavn", "", false));
            result.Add(new($"{rootElementName}.Meldingshode.Mottaker.Kommuneinfo.Bydelsinfo.Bydelsnummer", "", false));
            result.Add(new($"{rootElementName}.Meldingshode.Mottaker.Kommuneinfo.Bydelsinfo.Bydelsnavn", "", false));

            result.Add(new($"{rootElementName}.Meldingshode.KontaktInfoAvsender.Kontaktperson.Navn", "Bjørge Sæther", true));
            result.Add(new($"{rootElementName}.Meldingshode.KontaktInfoAvsender.Kontaktperson.Telefon", "90822239", true));
            result.Add(new($"{rootElementName}.Meldingshode.KontaktInfoAvsender.Kontaktperson.epost", "bjorge.saether@bufdir.no", true));
            result.Add(new($"{rootElementName}.Meldingshode.KontaktInfoAvsender.KontaktpersonLeder.Navn", "Kenneth Normann Hansen", true));
            result.Add(new($"{rootElementName}.Meldingshode.KontaktInfoAvsender.KontaktpersonLeder.Telefon", "90909090", true));
            result.Add(new($"{rootElementName}.Meldingshode.KontaktInfoAvsender.KontaktpersonLeder.epost", "kenneth.hansen@bufdir.no", true));


            result.Add(new($"{rootElementName}.Klient.Identifikator.Fodselsnummer", barn?.FREG_Person?.Fodselsnummer ?? "---", false));
            result.Add(new($"{rootElementName}.Klient.Identifikator.Fodseldato", barn?.FREG_Person?.Fodselsdato.ToString("yyyy-MM-dd") ?? "---", false));
            result.Add(new($"{rootElementName}.Klient.Identifikator.Kjonn", barn?.FREG_Person?.Kjonn ?? "---", false));
            result.Add(new($"{rootElementName}.Klient.Identifikator.EMA_FALSE", false.ToString(), false));
            result.Add(new($"{rootElementName}.Klient.Identifikator.TerminDato", "", false));
            result.Add(new($"{rootElementName}.Klient.Identifikator.Ufodt_FALSE", false.ToString(), false));
            result.Add(new($"{rootElementName}.Klient.Identifikator.DUFnummer", "", false));
            result.Add(new($"{rootElementName}.Klient.KommunalSaksId", DateTime.Now.Year.ToString() + "-" + barn?.FREG_Person?.Fodselsnummer, false));
            result.Add(new($"{rootElementName}.TiltakHistorikk", @"01.02.2022 - 12.09.2022 Fosterhjem
12.09.2022 - 20.10.2023 Omsorgsinstitusjon", true));
            return result;

        }
    }
}