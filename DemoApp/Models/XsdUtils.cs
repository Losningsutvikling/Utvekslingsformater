using DemoApp.Models.Json;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace DemoApp.Models
{
    public static class XsdUtils
    {
        public static readonly string TrueStringXsd = "true";

        public static List<XmlSchema> XsdSchemasWithRootElement { get; set; } = [];
        public static List<XmlSchema> MeldingsprotokollVersjoner { get; set; } = [];

        public static List<Kodelister> kodelister { get; set; } = [];
        public static Dictionary<string, Kodelister?> KodelisteFiler { get; set; } = [];

        public static string PreselectedProtocolVersion { get; set; } = "";

        public static TeksterJson Tekster { get; internal set; }

        private static string GetDefinitionsDirectory(IConfiguration config)
        {
            return config["DefsPath"]
                ?? throw new Exception("Finner ikke verdi for DefsPath i appsettings");
        }
        private static void SetPreselectedProtocolVersion(IConfiguration config)
        {
            PreselectedProtocolVersion = config["Protocol"] ?? "";
        }

        public static void ClearXsds()
        {
            kodelister.Clear();
            XsdSchemasWithRootElement.Clear();
        }

        public static void LoadXsds(IConfiguration config)
        {
            SetPreselectedProtocolVersion(config);
            string DefsPath = GetDefinitionsDirectory(config);
            var files = new DirectoryInfo(DefsPath).GetFiles(@"*.xsd", SearchOption.TopDirectoryOnly/*AllDirectories*/);
            XmlSchemaSet schemaSet = new();
            foreach (var file in files)
            {
                using var reader = XmlReader.Create(file.FullName);
                XmlSchema schema = XmlSchema.Read(reader, null)
                        ?? throw new Exception($"Kan ikke laste {file.Name}");
                schemaSet.Add(schema);
                if (file.Name.ToLower().StartsWith(Konstanter.BufdirMeldingProtokollFilnavn) && XsdUtils.ContainsTypeDefinition(schema, Konstanter.MeldingType))
                    MeldingsprotokollVersjoner.Add(schema);
            }
            schemaSet.Compile();
            foreach (XmlSchema schema in schemaSet.Schemas())
            {
                if (schema.Elements.Count > 0)
                {
                    XsdSchemasWithRootElement.Add(schema);
                }
                foreach (var nmsp in schema.Namespaces.ToArray())
                {
                    if (nmsp.Name == "kodelister")
                    {
                        KodelisteFiler[nmsp.Namespace] = null;
                    }
                }

            }
            LoadKodelister(config);

        }

        public static void LoadTekster(IConfiguration config)
        {
            string path = config["JsonPath"];
            var files = new DirectoryInfo(path).GetFiles(@"*.json", SearchOption.TopDirectoryOnly);
            var file = files.First(f => f.Name.IndexOf("schema") < 0);
            string jsonData = File.ReadAllText(file.FullName);
            Tekster = JsonSerializer.Deserialize<TeksterJson>(jsonData);
        }

        private static bool ContainsTypeDefinition(XmlSchema schema, string typeName)
        {
            var type = GetTypeDefinition(schema, typeName);
            return type != null;
        }

        public static XmlSchemaAnnotated? GetTypeDefinition(XmlSchema schema, string typeName)
        {
            foreach (var item in schema.Items)
            {
                if (item is XmlSchemaAnnotated typedef)
                {
                    if (GetName(typedef) == typeName)
                        return typedef;
                }
            }
            return null;
        }

        public static void LoadKodelister(IConfiguration config)
        {
            string defsPath = GetDefinitionsDirectory(config);
            var serializer = new XmlSerializer(typeof(Kodelister));
            // var files = new DirectoryInfo(defsPath).GetFiles(@"*odeliste*.xml", SearchOption.TopDirectoryOnly/*AllDirectories*/);
            var filnavn = KodelisteFiler.Select(kvp => kvp.Key);
            foreach (var fil in filnavn)
            {
                string fullName = $"{defsPath}\\{fil}";
                using var reader = new StreamReader(fullName);
                var innhold = serializer.Deserialize(reader) as Kodelister
                    ?? throw new Exception($"Kunne ikke lese innhold i kodelistefil {fullName}");
                kodelister.Add(innhold);
                KodelisteFiler[fil] = innhold;
            }
            InitKodelister();
        }

        private static void InitKodelister()
        {
            if (kodelister.Count == 0)
                throw new Exception("Kodelister er ikke lastet");
            foreach (Kodelister lister in kodelister)
            {
                foreach (Kodeliste liste in lister.kodelister ?? [])
                {
                    if (!string.IsNullOrEmpty(liste.utdragfra))
                    {
                        var kildeListe = GetKildeKodeliste(lister.nmsp ?? "", liste.utdragfra);
                        if (liste.koder != null)
                        {
                            foreach (var kode in liste.koder)
                            {
                                var kildeKode = (kildeListe?.koder ?? []).FirstOrDefault(k => k.verdi == kode.verdi)
                                    ?? throw new Exception($"Finner ikke kode {kode.verdi} i kildeliste {kildeListe.navn}");
                                kode.tekst = kildeKode.tekst;
                            }
                        }
                    }
                }
            }
        }

        public static Kodeliste? GetKildeKodeliste(string nmsp, string kilde, int level = 0)
        {
            if (level > 10)
            {
                throw new Exception($"Søk etter kildeliste {kilde} går i loop");
            }
            Kodeliste? liste = null;
            foreach (Kodelister kodelisterXsd in kodelister)
            {
                if (nmsp == kodelisterXsd.nmsp)
                {
                    liste = (kodelisterXsd.kodelister ?? []).FirstOrDefault(k => k.navn == kilde);
                    if (!string.IsNullOrEmpty(liste?.utdragfra))
                        liste = GetKildeKodeliste(nmsp, liste.utdragfra, level + 1);
                    if (liste == null)
                        throw new Exception($"Finner ikke kildeliste {kilde}");
                }
            }
            return liste;
        }

        public static Kodeliste? GetKodeliste(XmlSchemaAnnotated prop)
        {
            var simpleType = GetSimpleType(prop)
                ?? throw new Exception($"Finner ikke type {prop}");
            XmlSchema schema = GetSchema(prop);
            return GetKodeliste(schema, simpleType.Name!);
        }

        public static List<KeyValuePair<string, string>> GetKodelisteVerdier(XmlSchemaAnnotated prop)
        {
            List<KeyValuePair<string, string>> result = [];
            var simpleType = GetSimpleType(prop)
                ?? throw new Exception($"Finner ikke type {prop}");
            XmlSchema schema = GetSchema(prop);
            var kodeliste = GetKodeliste(schema, simpleType.Name!);
            if (kodeliste?.koder != null)
            {
                foreach (var kode in kodeliste.koder)
                {
                    if (!string.IsNullOrEmpty(kode.verdi) && !string.IsNullOrEmpty(kode.tekst))
                        result.Add(new(kode.verdi, kode.tekst));
                }
                return result;
            }
            else
                return GetEnumValues(prop);
        }



        public static Kodeliste? GetKodeliste(XmlSchema schema, string navn)
        {
            Kodelister? kodelisterXsd = kodelister.FirstOrDefault(kl => kl.nmsp == schema.TargetNamespace);
            var result = kodelisterXsd?.kodelister?.FirstOrDefault(k => k.navn == navn);
            return result;
        }

        public static List<XmlSchemaAnnotated> GetXsdElements(XmlSchemaAnnotated? prop, bool flat = false)
        {
            List<XmlSchemaAnnotated> list = [];
            if (prop != null)
                GetXsdElements(prop, flat, ref list);
            return list;
        }

        public static List<XmlSchema> GetSchemasForProtocol(string protocolNmsp)
        {
            return XsdSchemasWithRootElement.Where(sch => sch.Namespaces.ToArray().Any(nmsp => nmsp.Namespace.Equals(protocolNmsp))).ToList();
        }

        public static void GetXsdElements(XmlSchemaAnnotated prop, bool flat, ref List<XmlSchemaAnnotated> list)
        {
            list.Add(prop);
            if (flat && prop is XmlSchemaElement element)
                if (element.ElementSchemaType is XmlSchemaComplexType complexElement && flat)
                {
                    if (complexElement.Particle is XmlSchemaSequence sequenceElement)
                    {
                        foreach (XmlSchemaElement child in sequenceElement.Items.Cast<XmlSchemaElement>())
                        {
                            GetXsdElements(child, false, ref list);
                        }
                    }
                }
        }

        public static XmlSchema GetSchema(XmlSchemaObject obj)
        {
            if (obj is XmlSchemaElement element && element.ElementSchemaType != null)
                return GetSchema(element.ElementSchemaType);
            if (obj.Parent is XmlSchema schema)
                return schema;
            if (obj.Parent != null)
                return GetSchema(obj.Parent);
            throw new Exception("Finner ikke Schema som obj er definert i");
        }

        public static List<XmlSchemaAnnotated> GetXsdChildren(XmlSchemaAnnotated prop)
        {
            List<XmlSchemaAnnotated> list = [];
            if (prop is XmlSchemaElement element && element.ElementSchemaType is XmlSchemaComplexType complexElement)
            {
                if (complexElement.Particle is XmlSchemaSequence sequenceElement)
                {
                    foreach (XmlSchemaAnnotated child in sequenceElement.Items)
                    {
                        list.Add(child);
                    }
                }
                else if (complexElement.ContentTypeParticle is XmlSchemaSequence contentTypeSequence)
                {
                    foreach (XmlSchemaAnnotated child in contentTypeSequence.Items)
                    {
                        list.Add(child);
                    }
                }

            }
            return list;
        }
        public static List<XmlSchemaElement> GetXsdChildElements(XmlSchemaAnnotated prop)
        {
            List<XmlSchemaElement> list = [];
            if (prop is XmlSchemaElement element && element.ElementSchemaType is XmlSchemaComplexType complexElement)
            {
                if (complexElement.Particle is XmlSchemaSequence sequenceElement)
                {
                    foreach (XmlSchemaAnnotated child in sequenceElement.Items)
                    {
                        //                        TODO! Det er her et Choice - element dukker opp i lista sammen med alle XmlSchemaElement -typer!!!! må få med < choice >
                        if (child is XmlSchemaElement childElement)
                            list.Add(childElement);
                    }
                }
                else if (complexElement.ContentTypeParticle is XmlSchemaSequence contentTypeSequence)
                {
                    foreach (XmlSchemaAnnotated child in contentTypeSequence.Items)
                    {
                        if (child is XmlSchemaElement childElement)
                            list.Add(childElement);
                    }
                }

            }
            return list;
        }

        public static string GetControlNameForProperty(XmlSchemaAnnotated prop)
        {
            string controlName;
            XmlSchemaDatatype? datatype = null;
            var simpleType = GetSimpleType(prop);
            var hintedControlType = GetAppInfoValue(prop, "controlType");
            if (hintedControlType != "")
                return hintedControlType;
            if (XsdUtils.GetIsRepeating(prop))
            {
                return "Duplicator";
            }
            else if (prop is XmlSchemaChoice)
            {
                return "Choice";
            }
            else if (prop is XmlSchemaElement element)
            {
                if (element.ElementSchemaType is XmlSchemaComplexType)
                {
                    if (IsChoiceElement(element))
                        return "Choice";
                    if (IsOptionalBlock(element))
                        return "OptionalBlock";
                    return "Gruppe";
                }
                if (GetIsEnumType(simpleType))
                {
                    if (element.MaxOccurs > 1)
                    {
                        return "MultipleChoice";
                    }
                    return "SelectOne";
                }
                if (element?.ElementSchemaType?.Datatype == null)
                    throw new Exception("Datatype == null");
                datatype = element.ElementSchemaType.Datatype;
            }
            else if (prop is XmlSchemaAttribute)
            {
                datatype = simpleType?.Datatype ??
                    throw new Exception("simpleType?.Datatype == null");
            }

            controlName = datatype?.TypeCode switch
            {
                XmlTypeCode.Boolean => "Checkbox",
                XmlTypeCode.Date => "Dato",
                _ => "Tekst"
            };
            return controlName;
        }

        public static int GetMaxOccurs(XmlSchemaAnnotated prop)
        {
            if (prop is XmlSchemaElement element)
            {
                if (element.MaxOccurs > 1000)
                    return 1000;
                return Convert.ToInt32(element.MaxOccurs);
            }
            return 1000;
        }

        public static int GetMinOccurs(XmlSchemaAnnotated prop)
        {
            if (prop is XmlSchemaElement element)
            {
                return Convert.ToInt32(element.MinOccurs);
            }
            return -1;
        }

        public static bool GetIsRepeating(XmlSchemaAnnotated prop)
        {
            if (prop is XmlSchemaElement element)
            {
                return element.MinOccurs <= 1 && element.MaxOccurs > 1;
            }
            return false;
        }

        public static List<XmlSchemaObject>? GetChoiceElements(XmlSchemaAnnotated element)
        {
            List<XmlSchemaObject> result = [];
            if ((element as XmlSchemaElement)?.ElementSchemaType is XmlSchemaComplexType compType)
            {
                if (compType.Particle is XmlSchemaChoice choice)
                {
                    return GetChoiceElements(choice);
                }
            }
            else if (element is XmlSchemaChoice choice)
                return GetChoiceElements(choice);
            return null;
        }
        public static List<XmlSchemaObject> GetChoiceElements(XmlSchemaChoice choiceElement)
        {
            List<XmlSchemaObject> result = [];

            foreach (var item in choiceElement.Items)
                result.Add(item);
            return result;
        }
        public static bool IsChoiceElement(XmlSchemaElement element)
        {
            bool anyChoiceChildren = false;
            bool allChildrenChoice = true;
            if (element.ElementSchemaType is XmlSchemaComplexType compType)
            {
                if (compType.Particle is XmlSchemaChoice choice)
                    anyChoiceChildren = true;
                else
                    allChildrenChoice = false;
            }
            return anyChoiceChildren && allChildrenChoice;
        }
        public static XmlSchemaElement? FirstChoiceElement(XmlSchemaElement element)
        {
            bool anyChoiceChildren = false;
            bool allChildrenChoice = true;
            if (element.ElementSchemaType is XmlSchemaComplexType compType)
            {
                if (compType.Particle is XmlSchemaChoice choice)
                    anyChoiceChildren = true;
                else
                    allChildrenChoice = false;
            }
            if (anyChoiceChildren && allChildrenChoice)
                return element;
            var children = GetXsdChildElements(element);
            foreach (var child in children)
            {
                if (IsChoiceElement(child))
                {
                    return child;
                }
            }
            foreach (var child in children)
            {
                var firstChildChoiceelement = FirstChoiceElement(child);
                if (firstChildChoiceelement != null)
                {
                    return firstChildChoiceelement;
                }
            }
            return null;
        }

        public static ChoiceFork? GetChoiceForks(XmlSchemaElement Element)
        {
            XmlSchemaElement? fork = FirstChoiceElement(Element);
            ChoiceFork result = null;
            if (fork != null)
            {
                var list = GetChoiceElements(fork);
                List<ChoiceFork> childForks = [];
                foreach (var item in list)
                {
                    //                    XmlSchemaElement firstChildFork;
                    if (item is XmlSchemaElement childElement)
                    {
                        /*firstChildFork = FirstChoiceElement(childElement);
                        if (firstChildFork != null)
                        {
                            var child
                        }*/
                        childForks.Add(new(childElement, null));
                    }
                }
                result = new(fork, childForks);
            }
            return result;
        }


        private static bool IsOptionalBlock(XmlSchemaElement element)
        {
            return (element.MinOccurs == 0 && element.MaxOccurs == 1 && AnyMandatoryElements(element));
        }

        private static bool AnyMandatoryElements(XmlSchemaElement element)
        {
            var children = GetXsdChildren(element);
            foreach (var child in children)
            {
                if (child is XmlSchemaElement childElement)
                {
                    if (childElement.MinOccurs > 0)
                        return true;
                }
            }
            return false;
        }

        public static string GetFixedValue(XmlSchemaAnnotated prop)
        {
            if (prop is XmlSchemaElement element)
                return element.FixedValue ?? "";
            return "";
        }
        // aksjon bruker | som separator
        public static bool GetUsedInAksjon(XmlSchemaElement element, string aksjon)
        {
            var appInfoValue = GetAppInfoValue(element, Konstanter.MeldingsForbindelse);
            if (string.IsNullOrEmpty(appInfoValue))
                return false;
            return $"|{appInfoValue}|".Contains($"|{aksjon}|");
        }
        public static List<KeyValuePair<string, string>> GetEnumValues(XmlSchemaAnnotated prop)
        {
            var simpleType = GetSimpleType(prop);
            if (simpleType != null)
            {
                return GetEnumValues(simpleType);
            }
            throw new Exception($"{prop} is not an Enumeration type");
        }
        public static List<KeyValuePair<string, string>> GetEnumValues(XmlSchemaSimpleType simpleType)
        {
            var result = new List<KeyValuePair<string, string>>();
            if (simpleType.Content is XmlSchemaSimpleTypeRestriction restriction)
            {
                foreach (XmlSchemaEnumerationFacet facet in restriction.Facets.OfType<XmlSchemaEnumerationFacet>())
                {
                    if (facet.Value != null)
                        result.Add(new(facet.Value, facet.Value));
                }
            }
            return result;
        }
        public static bool GetIsEnumType(XmlSchemaSimpleType? simpleType)
        {
            if (simpleType?.Content is XmlSchemaSimpleTypeRestriction restriction)
            {
                if (restriction.Facets.OfType<XmlSchemaEnumerationFacet>().Any())
                {
                    return true;
                }
            }
            return false;
        }

        public static bool GetIsOptional(XmlSchemaAnnotated element)
        {
            if (element is XmlSchemaElement el)
                return el.MinOccurs == 0;
            return false;
        }

        public static XmlSchemaSimpleType? GetSimpleType(XmlSchemaAnnotated prop)
        {
            if (prop is XmlSchemaSimpleType simpleType)
                return simpleType;
            if (prop is XmlSchemaElement element && element.ElementSchemaType is XmlSchemaSimpleType elementSimpleType)
                return elementSimpleType;
            else if (prop is XmlSchemaAttribute attribute && attribute.AttributeSchemaType is XmlSchemaSimpleType attrSimpleType)
                return attrSimpleType;
            return null;
        }

        public static int GetMinLength(XmlSchemaAnnotated prop)
        {
            var simpleType = GetSimpleType(prop);
            if (simpleType != null)
            {
                if (simpleType.Content is XmlSchemaSimpleTypeRestriction restriction)
                {
                    foreach (var facet in restriction.Facets.OfType<XmlSchemaMinLengthFacet>())
                    {
                        if (int.TryParse(facet.Value, out int len))
                            return len;
                    }
                }
            }
            return 0;
        }
        public static int GetMaxLength(XmlSchemaAnnotated prop)
        {
            var simpleType = GetSimpleType(prop);
            if (simpleType != null)
            {
                if (simpleType.Content is XmlSchemaSimpleTypeRestriction restriction)
                {
                    foreach (var facet in restriction.Facets.OfType<XmlSchemaMaxLengthFacet>())
                    {
                        if (int.TryParse(facet.Value, out int len))
                            return len;
                    }
                }
            }
            return 0;
        }
        internal static string GetMarkupText(XmlNode?[]? markup)
        {
            if (markup != null && markup.Length > 0)
            {
                StringBuilder sb = new();
                foreach (var node in markup)
                {
                    if (node != null)
                    {
                        sb.Append(node.Value?.Trim() ?? "");
                    }
                }
                return sb.ToString();
            }
            return "";
        }

        private static string GetAppInfoElement(XmlNode?[]? markup, string elementName)
        {
            if (markup?.Length >= 1)
            {
                for (int i = 0; i < markup.Length; i++)
                {
                    if (markup[i]?.Name == elementName)
                    {
                        return markup[i]!.InnerText.Trim();
                    }
                }
            }
            return "";
        }



        public static string GetAppInfoValue(XmlSchemaAnnotated? xsdType, string appInfoElement, bool recurse = true)
        {
            XmlSchemaAnnotation? annotation = xsdType?.Annotation;
            // Hack for å kompensere at XmlSchemaChoice alltid har null i Annotation
            // Funker med dette "hacket":
            if (annotation == null && xsdType is XmlSchemaChoice)
            {
                int index = 0;
                for (int i = 0; i < (xsdType.Parent as XmlSchemaSequence)?.Items.Count; i++)
                {
                    var item = (xsdType.Parent as XmlSchemaSequence)?.Items[i];
                    if (item?.LineNumber == xsdType.LineNumber)
                    {
                        index = i; break;
                    }
                }
                if (index < 0)
                    throw new Exception("Finner ikke choice-element i parent.Items");
                annotation = ((xsdType.Parent as XmlSchemaSequence)?.Items[index] as XmlSchemaChoice)?.Annotation;
            }
            if (annotation != null)
            {
                foreach (var item in annotation.Items)
                {
                    if (item is XmlSchemaAppInfo appInfo)
                    {
                        string tmpValue = GetAppInfoElement(appInfo.Markup, appInfoElement);
                        if (tmpValue != "")
                            return tmpValue;
                    }
                }
            }
            if (recurse)
            {
                if (xsdType is XmlSchemaElement element)
                {
                    return GetAppInfoValue(element!.ElementSchemaType, appInfoElement, false);
                }
                else if (xsdType is XmlSchemaAttribute attribute)
                {
                    return GetAppInfoValue(attribute.AttributeSchemaType, appInfoElement, false);
                }
            }
            return "";
        }

        public static string? GetCaption(XmlSchemaAnnotated prop, bool fallbackToName, string? suffix = "")
        {
            if (prop == null) return
                    "<Prop=null>";
            var element = Tekster.henvisning.skjemaelement.Where(el => el.id == prop.Id).FirstOrDefault();
            if (element != null && element.ledetekst != "")
            {
                return element.ledetekst;
            }
            var result = XsdUtils.GetAppInfoValue(prop, Konstanter.Ledetekst);
            if (result == "" && fallbackToName)
            {
                return GetName(prop) + suffix;
            }
            return result + suffix;
        }

        public static string? GetName(XmlSchemaAnnotated prop)
        {
            if (prop is XmlSchemaElement element)
                return element.Name;
            else if (prop is XmlSchemaAttribute attribute)
                return attribute.Name;
            else if (prop is XmlSchemaType type)
                return type.Name;
            return "";
        }

        public static List<XmlSchemaAnnotated> FindByXPath(XmlSchemaAnnotated startingPoint, string xpath)
        {
            List<XmlSchemaAnnotated> result = [];
            var children = GetXsdChildren(startingPoint);
            foreach (XmlSchemaAnnotated child in children)
            {
                if (string.Compare(GetName(child), xpath, true) == 0)
                    result.Add(child);
                var grandchildren = FindByXPath(child, xpath);
                if (grandchildren.Count > 0)
                    result.AddRange(grandchildren);
            }
            return result;
        }

        public static XmlSchemaElement? FindByXPath(string protocolNmsp, string xpath)
        {
            var schemas = GetSchemasForProtocol(protocolNmsp);
            foreach (var schema in schemas)
            {
                foreach (var item in schema.Items)
                {
                    if (item is XmlSchemaElement element && element.QualifiedName.ToString() == xpath)
                        return element;
                }
            }
            return null;
        }

        public static XmlSchemaAnnotated GetChildByPath(XmlSchemaAnnotated startingPoint, string xpath)
        {
            string[] pathArr = xpath.Split('/', StringSplitOptions.RemoveEmptyEntries & StringSplitOptions.TrimEntries);
            var result = GetChildByPath(startingPoint, pathArr)
                ?? throw new Exception($"{GetName(startingPoint)}/{xpath} not found");
            return result;
        }
        internal static XmlSchemaAnnotated? GetChildByPath(XmlSchemaAnnotated startingPoint, string[] pathArr)
        {
            var child1stLevel = FindByXPath(startingPoint, pathArr[0]);
            if (child1stLevel.Count == 0)
                return null;
            if (pathArr.Length > 1) // neste nivå i path
                return GetChildByPath(child1stLevel[0], pathArr.Skip(1).ToArray());
            return child1stLevel[0];
        }

        internal static bool PropIsMandatory(XmlSchemaAnnotated prop)
        {
            if (prop is XmlSchemaElement element)
                return element.MinOccurs > 0;
            return false;
        }


        public static XmlSchemaElement? GetMeldingshode(IEnumerable<object> items, bool digDeeper)
        {
            foreach (var item in items)
            {
                if (item is XmlSchemaElement rootElement)
                {
                    var hode = GetMeldingshode(rootElement, false);
                    if (hode != null)
                        return rootElement;
                    var children = XsdUtils.GetXsdChildren(rootElement);
                    var element = children.FirstOrDefault(ch => XsdUtils.GetName(ch) == Konstanter.Meldingshode);
                    if (element != null)
                    {
                        return element as XmlSchemaElement;
                    }
                    else if (digDeeper) // ett nivå ned...
                    {
                        foreach (var child in children)
                        {
                            if (child is XmlSchemaElement childElement && childElement.ElementSchemaType is XmlSchemaComplexType complexElement)
                            {
                                var childItems = GetXsdChildren(complexElement);
                                var childHode = GetMeldingshode(childItems, false);
                                if (childHode != null)
                                    return childHode;
                            }
                        }
                    }
                }
            }
            return null;
        }

        internal static XmlSchemaElement? GetMeldingshode(XmlSchemaElement element, bool digDeeper)
        {
            if (XsdUtils.GetName(element) == Konstanter.Meldingshode)
                return element;
            var childItems = GetXsdChildren(element);
            return GetMeldingshode(childItems, digDeeper);
        }

        internal static List<XmlSchemaElement> ElementsWithMeldingshode(XmlSchema schema)
        {
            List<XmlSchemaElement> result = [];
            foreach (var item in schema.Items)
            {
                if (item is XmlSchemaElement rootElement)
                {
                    if (XsdUtils.GetMeldingshode(rootElement, true) != null)
                        result.Add(rootElement);
                }
            }
            return result;
        }

        public static bool Extends(XmlSchemaType? elementType, string ancestorType)
        {
            bool extends = elementType?.BaseXmlSchemaType?.Name == ancestorType;
            if (!extends && elementType?.BaseXmlSchemaType != null)
                extends = Extends(elementType.BaseXmlSchemaType, ancestorType);
            return extends;
        }


        public static bool Extends(XmlSchemaElement element, string ancestorType)
        {
            return Extends(element.ElementSchemaType, ancestorType);
        }
        public static List<XmlSchemaElement> ElementsExtending(XmlSchema schema, string ancestorType)
        {
            List<XmlSchemaElement> result = [];
            foreach (var item in schema.Items)
            {
                if (item is XmlSchemaElement rootElement)
                {
                    if (Extends(rootElement, ancestorType))
                        result.Add(rootElement);
                }
            }
            return result;
        }

        internal static string GetDescription(XmlSchemaAnnotated prop)
        {
            var element = Tekster.henvisning.skjemaelement.Where(el => el.id == prop.Id).FirstOrDefault();
            if (element != null && element.veiledning != "")
            {
                return element.veiledning;
            }
            return "";
        }
    }
}