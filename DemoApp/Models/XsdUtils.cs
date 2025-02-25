using DemoApp.Models.Json;
using System.Collections;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace DemoApp.Models
{
    public static class XsdUtils
    {
        public static readonly string TrueStringXsd = "true";

        public static readonly string DateTimeFormatXML = "yyyy-MM-ddTHH:mm:ssK";
        public static Dictionary<string, string> CustomRenderers { get; set; } = [];
        public static List<XmlSchema> XsdSchemas { get; set; } = [];
        public static List<XmlSchema> MeldingsprotokollVersjoner { get; set; } = [];

        public static List<Kodelister> kodelister { get; set; } = [];
        public static Dictionary<string, Kodelister?> KodelisteFiler { get; set; } = [];

        public static string PreselectedProtocolVersion { get; set; } = "";

        public static TeksterJson Tekster { get; internal set; } = new();

        public static string GetDefinitionsDirectory(IConfiguration config)
        {
            return config["DefsPath"]
                ?? throw new Exception("Finner ikke verdi for DefsPath i appsettings");
        }
        public static string GetCodelistDirectory(IConfiguration config)
        {
            return config["CodelistPath"]
                ?? throw new Exception("Finner ikke verdi for CodelistPath i appsettings");
        }
        private static void SetPreselectedProtocolVersion(IConfiguration config)
        {
            PreselectedProtocolVersion = config["Protocol"] ?? "";
        }

        public static void ClearXsds()
        {
            kodelister.Clear();
            XsdSchemas.Clear();
        }

        public static void LoadXsds(IConfiguration config, IWebHostEnvironment env)
        {
            SetPreselectedProtocolVersion(config);
            string configPath = GetDefinitionsDirectory(config);
            string DefsPath = (configPath.StartsWith("..")) ? configPath : Path.Combine(env.ContentRootPath, configPath);
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
                XsdSchemas.Add(schema);

                foreach (var nmsp in schema.Namespaces.ToArray())
                {
                    if (nmsp.Name == "kodelister")
                    {
                        KodelisteFiler[nmsp.Namespace] = null;
                    }
                }

            }
            LoadKodelister(config, env);

        }

        public static void LoadTekster(IConfiguration config, IWebHostEnvironment env)
        {
            string configPath = config["JsonPath"] ?? "";
            string path = (configPath.StartsWith("..")) ? configPath : Path.Combine(env.ContentRootPath, configPath);
            var files = new DirectoryInfo(path).GetFiles(@"*.json", SearchOption.TopDirectoryOnly);
            var file = files?.First(f => f.Name.IndexOf("schema") < 0);
            if (file != null)
            {
                string jsonData = File.ReadAllText(file.FullName) ?? "";
                Tekster = JsonSerializer.Deserialize<TeksterJson>(jsonData)
                    ?? throw new Exception($"Kunne ikke laste tekster (filnavn: {file.FullName})");
            }
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

        public static void LoadKodelister(IConfiguration config, IWebHostEnvironment env)
        {
            string configPath = GetCodelistDirectory(config);
            string defsPath = (configPath.StartsWith("..")) ? configPath : Path.Combine(env.ContentRootPath, configPath);
            var serializer = new XmlSerializer(typeof(Kodelister));
            // var files = new DirectoryInfo(defsPath).GetFiles(@"*odeliste*.xml", SearchOption.TopDirectoryOnly/*AllDirectories*/);
            var filnavn = KodelisteFiler.Select(kvp => kvp.Key);
            foreach (var fil in filnavn)
            {
                string pathSeparator = defsPath.Contains('\\') ? "\\" : "/";
                string fullName = $"{defsPath}{pathSeparator}{fil}";
                using var reader = new StreamReader(fullName);
                var innhold = serializer.Deserialize(reader) as Kodelister
                    ?? throw new Exception($"Kunne ikke lese innhold i kodelistefil {fullName}");
                kodelister.Add(innhold);
                KodelisteFiler[fil] = innhold;
            }
            //InitKodelister();
        }

        /*private static void InitKodelister()
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
        }*/

        /*        public static Kodeliste? GetKildeKodeliste(string nmsp, string kilde, int level = 0)
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
        */
        public static Kodeliste? GetKodeliste(XmlSchemaAnnotated prop)
        {
            var simpleType = GetSimpleType(prop)
                ?? throw new Exception($"Finner ikke type {prop}");
            XmlSchema schema = GetSchema(prop);
            Kodeliste? kodeliste = GetKodeliste(schema, simpleType.Name!);
            if (kodeliste == null) // En restricted utgave av enumeration
            {
                var ancestorSimpleType = GetSimpleType(simpleType.BaseXmlSchemaType);
                if (GetIsEnumType(ancestorSimpleType))
                {
                    schema = GetSchema(ancestorSimpleType!);
                    kodeliste = GetKodeliste(schema, ancestorSimpleType!.Name!);
                    if (kodeliste != null)
                    {
                        var kodelisteCopyJSON = JsonSerializer.Serialize<Kodeliste>(kodeliste);
                        kodeliste = JsonSerializer.Deserialize<Kodeliste>(kodelisteCopyJSON)!;

                        List<Kode> filtered = [];
                        var values = GetEnumValues(simpleType);

                        foreach (var kode in kodeliste.koder!)
                        {
                            if (values.Exists(kvp => kvp.Key == kode.verdi))
                            {
                                filtered.Add(kode);
                            }
                        }
                        kodeliste.koder = filtered.ToArray();
                    }
                }
            }
            return kodeliste;
        }

        public static List<Kode> GetFilteredKodeliste(Kodeliste kodeliste, string variabel, string variabelVerdi)
        {
            List<Kode> result = [];
            if (kodeliste.variabler?.Length > 0 && kodeliste.variabler.Any(v => v.navn == variabel))
            {
                foreach (var kode in kodeliste.koder!)
                {
                    if (kode.variabler.Any(v => v.navn == variabel && v.verdi == variabelVerdi))
                        result.Add(kode);
                }
                if (result.Count == 0)
                    throw new Exception($"Tom kodefilterliste '{kodeliste.navn}' filtrert på {variabel}={variabelVerdi}");
            }
            else
                result = kodeliste.koder!.ToList();
            return result;
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
            return XsdSchemas.Where(sch => sch.Namespaces.ToArray().Any(nmsp => nmsp.Namespace.Equals(protocolNmsp))).ToList();
        }

        public static XmlSchema GetSchema(string targetNmsp)
        {
            var result = XsdSchemas.First(sch => sch.TargetNamespace == targetNmsp);
            return result;
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
            XmlSchemaComplexType? elementType = null;
            if (prop is XmlSchemaElement element && element.ElementSchemaType is XmlSchemaComplexType complexElement)
                elementType = complexElement;
            if (elementType == null && prop is XmlSchemaComplexType compType)
                elementType = compType;
            if (elementType != null)
            {
                if (elementType.Particle is XmlSchemaSequence sequenceElement)
                {
                    foreach (XmlSchemaAnnotated child in sequenceElement.Items)
                    {
                        list.Add(child);
                    }
                }
                else if (elementType.ContentTypeParticle is XmlSchemaSequence contentTypeSequence)
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
                    foreach (XmlSchemaAnnotated child in contentTypeSequence.Items.Cast<XmlSchemaAnnotated>())
                    {
                        if (child is XmlSchemaElement childElement)
                            list.Add(childElement);
                    }
                }

            }
            return list;
        }

        public static XmlSchemaSimpleType? GetIterateTypeDefinition(XmlSchemaAnnotated prop)
        {
            var constraint = XsdUtils.GetUniqueConstraint(prop);
            if (constraint != null)
            {
                var iteratorProp = constraint.Selector!.XPath
                    ?? throw new Exception($"Ingen selector i unique constraint i element {XsdUtils.GetName(prop)}");
                var enumProp = (iteratorProp == ".") ? prop : XsdUtils.GetChildByPath(prop, iteratorProp);
                if (enumProp != null)
                    return XsdUtils.GetSimpleType(enumProp);
            }
            return null;
        }


        public static string GetControlNameForProperty(XmlSchemaAnnotated prop, bool isAChoiceOption = false)
        {
            if (prop.Id != null && CustomRenderers.TryGetValue(prop.Id, out string? customRenderer))
                return customRenderer;
            string controlName;
            XmlSchemaDatatype? datatype = null;
            var simpleType = GetSimpleType(prop);
            if (GetIsRepeating(prop))
            {
                if (HasUniqueConstraint(prop))
                    return "Iterator";
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

            if (isAChoiceOption && datatype?.TypeCode == XmlTypeCode.Boolean)
                return "FixedBoolean";

            controlName = datatype?.TypeCode switch
            {
                XmlTypeCode.Boolean => "Checkbox",
                XmlTypeCode.Date => "Dato",
                _ => "Tekst"
            };
            return controlName;
        }

        public static XmlSchemaUnique? GetUniqueConstraint(XmlSchemaAnnotated prop)
        {
            if (prop is XmlSchemaElement element)
            {
                foreach (var constraint in element.Constraints)
                {
                    if (constraint is XmlSchemaUnique result)
                        return result;
                }
            }
            return null;
        }

        public static XmlSchemaUnique? GetUniqueConstraintChild(XmlSchemaAnnotated prop)
        {
            var children = GetXsdChildren(prop);
            if (children != null)
            {
                foreach (var child in children)
                {
                    var uniqueConstraint = GetUniqueConstraint(child);
                    if (uniqueConstraint != null)
                        return uniqueConstraint;
                }
                if (prop is XmlSchemaElement element)
                {
                    foreach (var constraint in element.Constraints)
                    {
                        if (constraint is XmlSchemaUnique result)
                            return result;
                    }
                }
            }
            return null;
        }


        public static bool HasUniqueConstraint(XmlSchemaAnnotated prop)
        {
            var constraint = GetUniqueConstraint(prop);
            return constraint != null;
        }

        internal static bool HasUniqueConstraintChild(XmlSchemaAnnotated schemaChild)
        {
            var unique = GetUniqueConstraintChild(schemaChild);
            return unique != null;
        }

        public static int GetMaxOccurs(XmlSchemaAnnotated prop)
        {
            if (prop is XmlSchemaElement element)
            {
                if (element.MaxOccurs > 1000)
                    return 1000;
                return Convert.ToInt32(element.MaxOccurs);
            }
            else if (prop is XmlSchemaChoice choice)
            {
                return Convert.ToInt32(choice.MaxOccurs);
            }
            return 1;
        }

        public static int GetMinOccurs(XmlSchemaAnnotated prop)
        {
            if (prop is XmlSchemaElement element)
            {
                return Convert.ToInt32(element.MinOccurs);
            }
            else if (prop is XmlSchemaChoice choice)
            {
                return Convert.ToInt32(choice.MinOccurs);
            }
            return 0;
        }

        public static bool GetIsRepeating(XmlSchemaAnnotated prop)
        {
            if (prop is XmlSchemaElement element)
            {
                return element.MaxOccurs > 1;
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

        public static bool IsSimpleType(XmlSchemaAnnotated prop)
        {
            if (prop is XmlSchemaSimpleType)
                return true;
            if (prop is XmlSchemaElement element && element.ElementSchemaType is XmlSchemaSimpleType)
                return true;
            return false;

        }

        public static bool IsChoiceElement(XmlSchemaAnnotated? element)
        {
            if (element == null)
                return false;
            return element is XmlSchemaChoice;
        }

        /*        public static bool IsChoiceElement(XmlSchemaComplexType? elementType)
                {
                    if (elementType == null)
                        return false;
                    bool anyChoiceChildren = false;
                    bool allChildrenChoice = true;
                    if (elementType.Particle is XmlSchemaChoice choice)
                        anyChoiceChildren = true;
                    else
                        allChildrenChoice = false;
                    return anyChoiceChildren && allChildrenChoice;
                }

                public static bool IsChoiceElement(XmlSchemaElement? element)
                {
                    if (element is null)
                        return false;
                    if (element.ElementSchemaType is XmlSchemaComplexType compType)
                    {
                        return IsChoiceElement(compType);
                    }
                    return false;
                }
        */
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
            var appInfoValue = GetAppInfoValue(element, Konstanter.MeldingsForbindelse, false, true);
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

        public static XmlSchemaSimpleType? GetSimpleType(XmlSchemaAnnotated? prop)
        {
            if (prop is XmlSchemaSimpleType simpleType)
                return simpleType;
            if (prop is XmlSchemaElement element && element.ElementSchemaType is XmlSchemaSimpleType elementSimpleType)
                return elementSimpleType;
            else if (prop is XmlSchemaAttribute attribute && attribute.AttributeSchemaType is XmlSchemaSimpleType attrSimpleType)
                return attrSimpleType;
            return null;
        }
        public static XmlSchemaComplexType? GetComplexType(XmlSchemaAnnotated prop)
        {
            if (prop is XmlSchemaComplexType complexType)
                return complexType;
            if (prop is XmlSchemaElement element && element.ElementSchemaType is XmlSchemaComplexType elementComplexType)
                return elementComplexType;
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

        public static string GetRegExRestriction(XmlSchemaAnnotated prop)
        {
            var simpleType = GetSimpleType(prop);
            if (simpleType != null)
            {
                if (simpleType.Content is XmlSchemaSimpleTypeRestriction restriction)
                {
                    foreach (var facet in restriction.Facets.OfType<XmlSchemaPatternFacet>())
                    {
                        if (!string.IsNullOrEmpty(facet.Value))
                            return facet.Value;
                    }
                }
            }
            return "";
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

        private static string GetAppInfoElement(XmlNode?[]? markup, string elementName, bool selectXml)
        {
            if (markup?.Length >= 1)
            {
                for (int i = 0; i < markup.Length; i++)
                {
                    if (markup[i]?.Name == elementName)
                    {
                        return (selectXml) ? markup[i]!.OuterXml : markup[i]!.InnerText.Trim();
                    }
                }
            }
            return "";
        }



        public static string GetAppInfoValue(XmlSchemaAnnotated? xsdType, string appInfoElement, bool selectXml, bool recurse)
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
                        string tmpValue = GetAppInfoElement(appInfo.Markup, appInfoElement, selectXml);
                        if (tmpValue != "")
                            return tmpValue;
                    }
                }
            }
            if (recurse)
            {
                if (xsdType is XmlSchemaElement element)
                {
                    return GetAppInfoValue(element!.ElementSchemaType, appInfoElement, false, false);
                }
                else if (xsdType is XmlSchemaAttribute attribute)
                {
                    return GetAppInfoValue(attribute.AttributeSchemaType, appInfoElement, false, false);
                }
            }
            return "";
        }

        public static string GetCaption(XmlSchemaAnnotated? prop, bool fallbackToName, string? suffix = "")
        {
            if (prop == null) return
                    "<Prop=null>";
            var element = Tekster.henvisning?.skjemaelement.Where(el => el.id == prop.Id).FirstOrDefault();
            if (element == null)
            {
                string entry = $"{prop.Id} - {GetName(prop)}";
                if (Tekster.missing.IndexOf(entry) < 0)
                    Tekster.missing.Add(entry);
            }
            if (element != null && element.ledetekst != "")
            {
                string entry = $"{prop.Id} - {GetName(prop)}";
                if (Tekster.used.IndexOf(entry) < 0)
                    Tekster.used.Add(entry);
                else if (element.navn != GetName(prop) && Tekster.wrongName.IndexOf(entry) < 0)
                    Tekster.wrongName.Add(entry);
                return element.ledetekst;
            }
            if (fallbackToName)
            {
                return GetName(prop) + suffix;
            }
            return "";
        }

        public static string GetKodelisteBeskrivelseFromVeiledning(string kodeliste_id, string element_id, string verdi)
        {
            var tekstelement = Tekster.henvisning?.kodelistetekster.Where(el => el.id == kodeliste_id && el.element_id == element_id).FirstOrDefault();
            if (tekstelement != null)
            {
                var kodelisteverdi = tekstelement.verdier.Where(kv => kv.verdi == verdi).FirstOrDefault();
                if (kodelisteverdi != null && !string.IsNullOrEmpty(kodelisteverdi.beskrivelse))
                    return kodelisteverdi.beskrivelse;
            }
            return "";
        }

        public static List<EnrichedElement> GetEnrichedChildren(XmlSchemaAnnotated prop)
        {
            List<EnrichedElement> result = [];
            var elements = XsdUtils.GetXsdChildren(prop);
            for (int i = 0; i < elements.Count; i++)
            {
                var element = Tekster.henvisning?.skjemaelement.Where(el => el.id == elements[i].Id).FirstOrDefault();
                EnrichedElement enriched = new(elements[i])
                {
                    OrigSort = i,
                    SortOrder = element?.sortering ?? 100
                };
                result.Add(enriched);
            }
            return result.OrderBy(e => e.SortOrder).ThenBy(e => e.OrigSort).ToList();
        }

        public static string GetName(XmlSchemaAnnotated? prop)
        {
            if (prop == null)
                return "";
            if (prop is XmlSchemaElement element)
                return element.Name ?? "";
            else if (prop is XmlSchemaUnique unique)
                return unique.Name ?? "";
            else if (prop is XmlSchemaAttribute attribute)
                return attribute.Name ?? "";
            else if (prop is XmlSchemaType type)
                return type.Name ?? "";
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
        public static XmlSchemaElement? ElementsExtending(XmlSchema schema, string ancestorType)
        {
            var result = GetRootElement(schema);
            return (result != null && Extends(result, ancestorType)) ? result : null;
        }

        internal static string GetDescription(XmlSchemaAnnotated prop)
        {
            var element = Tekster.henvisning?.skjemaelement.Where(el => el.id == prop.Id).FirstOrDefault();
            if (element != null && element.veiledning != "")
            {
                return element.veiledning;
            }
            return "";
        }

        internal static string GetQualifiedNamespace(XmlSchemaAnnotated elementType)
        {
            if (elementType is XmlSchemaElement schemaElement)
                return schemaElement.QualifiedName.Namespace;
            if (elementType is XmlSchemaSimpleType simpleType)
                return simpleType.QualifiedName.Namespace;
            if (elementType is XmlSchemaComplexType complexType)
                return complexType.QualifiedName.Namespace;
            return "";
        }

        internal static void SetCustomRenderers()
        {
            CustomRenderers["BUF_CC9A7CDF-B998-415D-995A-6275CBCCA465"] = "ElementSpecific/BarnetsSituasjon";
        }


        internal static bool ChoiceElementMandatory(XmlSchemaAnnotated prop)
        {
            if (prop is XmlSchemaChoice choiceElement)
            {
                return choiceElement.MinOccurs > 0;
            }
            return false;
        }


        internal static bool AllChoiceElementsSimpleAndMandatory(XmlSchemaAnnotated prop)
        {
            if (prop is XmlSchemaElement schemaElement)
            {
                bool allChildrenMandatory = true;
                if (IsChoiceElement(schemaElement))
                {
                    var children = GetXsdChildren(schemaElement);
                    foreach (var child in children)
                    {
                        if (!PropIsMandatory(child))
                        {
                            allChildrenMandatory = false;
                            break;
                        }
                    }
                    return allChildrenMandatory;
                }
            }
            return false;
        }

        public static string GetTargetNmsp(Dictionary<string, string> Meldingshode)
        {
            if (Meldingshode.ContainsKey(Konstanter.MeldingstypeNmsp))
                return Meldingshode[Konstanter.MeldingstypeNmsp];
            return "";
        }

        public static XmlSchema? SchemaFromTargetNmsp(string? nmsp)
        {
            if (nmsp == null)
                return null;
            var schema = XsdSchemas.FirstOrDefault(sch => sch.TargetNamespace == nmsp);
            if (schema != null)
                return schema;
            return null;
        }

        public static XmlSchema? SchemaProtocolFromSchema(XmlSchema schema)
        {
            var nmsp = GetSchemaProtocolNmsp(schema);
            return SchemaFromTargetNmsp(nmsp);
        }

        public static XmlSchema GetProtocolSchema(XmlSchema schema)
        {
            string protocolNmsp = GetSchemaProtocolNmsp(schema);
            return GetSchema(protocolNmsp);
        }

        public static string GetSchemaProtocolNmsp(XmlSchema schema)
        {
            var protocolNmsp = schema.Namespaces.ToArray().FirstOrDefault(nmsp => nmsp.Name == "mld");
            if (protocolNmsp != null)
                return protocolNmsp.Namespace;
            return "";
        }

        public static XmlSchemaElement? GetRootElement(XmlSchema? selectedSkjema)
        {
            if (selectedSkjema?.Elements.Count == 1)
            {
                // Litt odd, fant ingen annen måte å returnere første (eneste!) element....
                foreach (var element in selectedSkjema.Elements)
                {
                    var entry = (DictionaryEntry)element;
                    return entry.Value as XmlSchemaElement;
                }
            }
            return null;
        }

        public static XmlSchema[] XsdSchemasWithNamedRootElement(string name)
        {
            return XsdSchemas.Where(sch => (GetRootElement(sch)?.Name ?? "-") == name).ToArray();
        }

        public static string[] XsdSchemaNamesWithNamedRootElement(string name)
        {
            var schemas = XsdSchemasWithNamedRootElement(name);
            return schemas.Select(sch => sch.TargetNamespace!).ToArray();
        }

        internal static string GetChoiceElementNames(XmlSchemaChoice? element, string delimiter = "_")
        {
            if (element == null)
                return "";
            List<string> names = [];
            foreach (var item in element.Items)
            {
                names.Add(GetName(item as XmlSchemaAnnotated));
            }
            return string.Join(delimiter, names);
        }

        internal static bool GetPatternOK(string pattern, string value)
        {
            Regex regex = new(pattern);
            return regex.IsMatch(value);
        }

        internal static XmlSchemaAnnotated? GetParent(XmlSchemaAnnotated schType)
        {
            if (schType.Parent is XmlSchemaAnnotated ann && ann.Parent is XmlSchemaAnnotated annParent)
                return annParent;
            return null;
        }
    }
}