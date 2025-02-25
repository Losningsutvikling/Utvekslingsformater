using DemoApp.Models.Simulator;
using System.Xml;
using System.Xml.Schema;

namespace DemoApp.Models
{
    public static class XmlFactory
    {
        /// <summary>
        /// {0} = Organisasjonsnummer
        /// {1} = lokasjon/status => 'sendt', 'mottatt', 'lagret'
        /// {2} = Filnavn (se neste konstant)        
        /// /// </summary>
        public static string MELDING_PATH = "{0}/{1}/{2}";


        /// <summary>
        /// {0} = meldings-id
        /// {1} = Dato/tid 
        /// {2} = meldingstype (nmsp - filnavndel, f.eks.: 'Bufdir_Barnevern_Henvisning_Familierad_v0.9.0')
        /// </summary>
        public static readonly string MELDING_FILNAVN_PATTERN = "[{0}] {1} ({2}).xml";

        public static readonly string MELDING_FILNAVN_DATETIME_FORMAT = "yyyy-MM-dd_HH-mm-ss";

        public static string GetMeldingPath(string orgNummer, string subFolder)
        {
            return $"{orgNummer}/{subFolder}/";
        }

        public static string GenerateFileName(string nmsp, Dictionary<string, string> QueryParams, string subFolder, DateTime dateTime)
        {
            string orgNr = Utils.GetRequestValuePartialKey(QueryParams, Konstanter.OrgnrPartialPath);
            string fileNamePart = string.Format(XmlFactory.MELDING_FILNAVN_PATTERN,
                        Utils.GetRequestValuePartialKey(QueryParams, Konstanter.MeldingsIdPartialPath),
                        dateTime.ToString(XmlFactory.MELDING_FILNAVN_DATETIME_FORMAT),
                        Utils.GetRequestValue(QueryParams, Konstanter.SelectedSkjema).Replace("https://", "").Replace("/", "."));

            string fileName = string.Format(XmlFactory.MELDING_PATH, orgNr, subFolder, fileNamePart);
            return fileName;
        }

        /// <summary>
        /// Returnerer lagret filnavn (kan avvike ved valideringsfeil)
        /// </summary>
        /// <param name="fileName">Hvis filnavn inneholder '/sendt/' blir dette erstattet med '/lagret' dersom det er valideringsfeil</param>
        /// <param name="errors">Hvis denne sendes med, gjøres validering</param>

        public static string WriteXml(IConfiguration config, IWebHostEnvironment env, bool useNmsp, Dictionary<string, string> values, string fileName, List<XmlValidationError>? errors = null)
        {
            Dictionary<string, string> ns = [];

            string selectedProtocolName = Utils.GetRequestValue(values, Konstanter.SelectedProtocol);
            //            var meldingsProtokoll = XsdUtils.MeldingsprotokollVersjoner.FirstOrDefault(v => v.Version == selectedProtocolName);
            var schemaNmsp = Utils.GetRequestValue(values, Konstanter.SelectedSkjema);
            var xsdSchema = XsdUtils.SchemaFromTargetNmsp(schemaNmsp)
                ?? throw new Exception($"Finner ikke XSD med nmsp = '{schemaNmsp}'");
            var schemaElement = XsdUtils.GetRootElement(xsdSchema)
                ?? throw new Exception($"Ingen rotelement i {schemaNmsp}");

            string configPath = XsdUtils.GetDefinitionsDirectory(config);
            string DefsPath = (configPath.StartsWith("..")) ? configPath : Path.Combine(env.ContentRootPath, configPath);
            var files = new DirectoryInfo(DefsPath).GetFiles(@"*.xsd", SearchOption.TopDirectoryOnly/*AllDirectories*/);
            XmlSchemaSet schemaSet = new();

            string schemaLoc = "";

            foreach (var file in files)
            {
                using var reader = XmlReader.Create(file.FullName);
                XmlSchema schema = XmlSchema.Read(reader, null)
                        ?? throw new Exception($"Kan ikke laste {file.Name}");
                schemaSet.Add(schema);
                if (schemaElement!.SourceUri!.Contains(file.Name) == true)
                {
                    schemaLoc = file.Name;
                };
            }
            schemaSet.Compile();

            XmlDocument xmlDoc = new XmlDocument();
            if (useNmsp)
                xmlDoc.Schemas.Add(schemaSet);
            XmlNode docNode = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            xmlDoc.AppendChild(docNode);

            Dictionary<string, string> rootElementAttributes = [];

            if (useNmsp)
            {
                foreach (XmlSchema sch in schemaSet.Schemas())
                {
                    foreach (var nms in sch.Namespaces.ToArray().ToList())
                    {
                        if (nms.Name != "" && nms.Namespace.StartsWith("https://Bufdir.no") && !ns.ContainsKey(nms.Namespace))
                        {
                            ns.Add(nms.Namespace, nms.Name);
                            rootElementAttributes["xmlns:" + nms.Name] = nms.Namespace;
                        }
                    }
                }
            }

            if (useNmsp)
            {
                ns.Add(schemaElement.QualifiedName.Namespace, "melding");
            }

            if (useNmsp)
            {
                rootElementAttributes["xmlns"] = schemaElement.QualifiedName.Namespace;
            }

            rootElementAttributes["xmlns:xsi"] = "http://www.w3.org/2001/XMLSchema-instance";

            WriteElement(useNmsp, xmlDoc, null, "", schemaElement, ns, schemaElement.QualifiedName.Namespace, values);

            var rootDocumentElement = xmlDoc.ChildNodes[1] as XmlElement
                ?? throw new Exception($"Rotelement '{schemaElement.Name}' ikke tilstede i dokument av typen '{schemaElement.QualifiedName.Namespace}'");


            foreach (var attr in rootElementAttributes)
            {
                rootDocumentElement.SetAttribute(attr.Key, attr.Value);
            }

            // schemalocation må legges til etter rotelement har xsi namespace for kunne få xsi prefix
            rootDocumentElement.SetAttribute("schemaLocation", "http://www.w3.org/2001/XMLSchema-instance", xsdSchema.TargetNamespace + " " + schemaLoc);

            if (errors != null)
                XmlValidator.ValidateXml(xmlDoc, xsdSchema!, ns, errors);
            string xmlContent = xmlDoc.OuterXml;
            if (errors?.Count == 0)
            {
                bool valid = XmlValidator.ValidateXmlBuiltIn(xmlDoc, out string error);
                if (!valid)
                    errors.Add(new(null, XmlValidationErrorType.StandardXmlValidationError, error));
            }
            if (errors != null && errors.Count > 0)
                fileName = fileName.Replace("/sendt/", "/lagret/");
            var task = BlobShareReaderWriter.SaveFile(config, fileName, xmlContent);
            task.Wait();
            return fileName;
        }


        public static bool WriteElement(bool useNmsp, XmlDocument xmlDoc, XmlElement? parent, string xmlPath, XmlSchemaAnnotated elementType,
                                        Dictionary<string, string> ns, string defaultNmsp, Dictionary<string, string> values, string index = "")
        {
            bool result = false;
            if (XsdUtils.GetIsRepeating(elementType) && index == "")
            {
                string[] indexValues = GetIndexValues(values, xmlPath, elementType);
                foreach (var indexValue in indexValues)
                {
                    if (WriteElement(useNmsp, xmlDoc, parent, xmlPath, elementType, ns, defaultNmsp, values, indexValue))
                    {
                        result = true;
                    }
                }
            }
            else
            {
                string indexPart = (index == "") ? "" : $":{index}";
                var newPath = $"{xmlPath.Trim('.')}.{XsdUtils.GetName(elementType).TrimStart('.')}{indexPart}";
                if (elementType is XmlSchemaChoice choiceElement)
                {
                    string elementName = $"{xmlPath.Trim('.')}.{XsdUtils.GetChoiceElementNames(choiceElement)}_CHOICE";
                    if (values.TryGetValue(elementName, out string? chosen))
                    {
                        foreach (XmlSchemaElement item in choiceElement.Items)
                        {
                            if (item.Name == chosen)
                            {
                                if (WriteElement(useNmsp, xmlDoc, parent, newPath, item, ns, defaultNmsp, values))
                                {
                                    result = true;
                                }
                            }
                        }
                    }
                }
                else
                {
                    string nmsp = XsdUtils.GetQualifiedNamespace(elementType);
                    var elementPrefix = (useNmsp) ? ns[nmsp] : "";
                    var elementNmsp = (useNmsp) ? nmsp : defaultNmsp;
                    XmlElement element = xmlDoc.CreateElement(elementPrefix, XsdUtils.GetName(elementType), elementNmsp);

                    if (XsdUtils.GetComplexType(elementType) != null)
                    {
                        var complexType = XsdUtils.GetComplexType(elementType);
                        foreach (var child in XsdUtils.GetXsdChildren(complexType!))
                        {
                            if (WriteElement(useNmsp, xmlDoc, element, newPath, child, ns, defaultNmsp, values))
                            {
                                result = true;
                            }
                        }
                    }
                    else // is SimpleType
                    {
                        string input;

                        if (XsdUtils.GetControlNameForProperty(elementType) == "Checkbox" && values.ContainsKey($"{newPath}_FALSE") && !(values.ContainsKey(newPath)))
                        {
                            element.InnerText = "false";
                            result = true;
                        }

                        else if (values.ContainsKey(newPath))
                        {
                            input = values[newPath];
                            if (input != "")
                            {
                                element.InnerText = input == "on" ? "true" : input;
                                result = true;
                            }
                        }
                    }
                    if (result)
                    {
                        if (parent != null)
                            parent.AppendChild(element);
                        else
                            xmlDoc.AppendChild(element);
                    }
                }
            }
            return result;
        }

        private static string[] GetIndexValues(Dictionary<string, string> values, string xmlPath, XmlSchemaAnnotated elementType)
        {
            List<string> result = [];
            string elementPath = $"{xmlPath}.{XsdUtils.GetName(elementType)}:";
            var selection = values.Where(kvp => kvp.Key.StartsWith(elementPath));
            foreach (var kvp in selection)
            {
                string key = kvp.Key.Replace(elementPath, "");
                int endIndex = key.IndexOf(".");
                key = (endIndex > 0) ? key.Substring(0, endIndex) : key;
                if (key != "" && !result.Contains(key))
                    result.Add(key);
            }
            return result.ToArray();
        }

        public static void ReadFilesMeldingshode(IConfiguration config, string folder, List<FilInfo> files, string filter = "")
        {
            var fileNames = BlobShareReaderWriter.GetAllFiles(config, folder); // alle lagrede filer
            var filterParts = filter.Split("=");

            foreach (var file in fileNames)
            {
                var fileInfo = ReadFileMeldingshode(config, file);
                bool addThisFile = (filterParts.Length == 2) ? fileInfo.Meldingshode.GetValue(filterParts[0]) == filterParts[1] : true;
                if (addThisFile)
                    files.Add(fileInfo);
            }
        }

        private static void ReadMeldingshodeElement(XmlReader reader, object current, string localName)
        {
            while (reader.Read())
            {
                if (reader.LocalName == localName) // elementets slutt-tag
                {
                    break;
                }

                var propInfo = current.GetType().GetProperty(reader.LocalName);
                if (propInfo != null)
                {
                    if (!propInfo.PropertyType.FullName!.StartsWith("System."))
                    {
                        var childObj = propInfo.GetValue(current);
                        if (childObj == null)
                        {
                            Type objPropType = (propInfo.PropertyType.GenericTypeArguments.Length == 0) ? propInfo.PropertyType : propInfo.PropertyType.GenericTypeArguments[0];
                            childObj = Activator.CreateInstance(objPropType)!;
                            propInfo.SetValue(current, childObj);
                        }
                        ReadMeldingshodeElement(reader, childObj, reader.LocalName);
                    }
                    else
                    {
                        var text = reader.ReadString();
                        if (text != "")
                        {
                            if (propInfo.PropertyType == typeof(DateTime) || (propInfo.PropertyType.GenericTypeArguments.Length == 1 && propInfo.PropertyType.GenericTypeArguments[0] == typeof(DateTime)))
                            {
                                if (DateTime.TryParseExact(text.Trim(), XsdUtils.DateTimeFormatXML, null, System.Globalization.DateTimeStyles.None, out DateTime dtResult))
                                    propInfo.SetValue(current, dtResult);
                            }
                            else
                                propInfo.SetValue(current, text);
                        }
                    }
                }
            }

        }

        public static FilInfo ReadFileMeldingshode(IConfiguration config, string fileName)
        {
            FilInfo result = new() { Filnavn = fileName };
            XmlReader reader;

            if (string.IsNullOrEmpty(config["StorageAccountConnectString"]) && string.IsNullOrEmpty(config["StorageAccountClientId"])) // hvis ingen storage account emulator, burk lokal filsystem
            {
                reader = XmlReader.Create(fileName);
            }
            else
            {
                var xmlfile = BlobShareReaderWriter.GetFileStreamFromBlob(config, fileName).Result;
                reader = XmlReader.Create(xmlfile);
            }

            while (reader.Read())
            {
                if (reader.IsStartElement("mld:Meldingshode") || reader.IsStartElement("Meldingshode"))
                {
                    ReadMeldingshodeElement(reader, result.Meldingshode, "Meldingshode");
                    break;
                }
            }
            return result;
        }

        public static string GetDateTimePartFromFilename(string fileName)
        {
            var parts = fileName.Split(' ');
            if (parts.Length == 3)
            {
                string strDate = parts[1];
                if (DateTime.TryParseExact(strDate.Trim(), MELDING_FILNAVN_DATETIME_FORMAT, null, System.Globalization.DateTimeStyles.None, out DateTime result))
                    return result.ToString(MELDING_FILNAVN_DATETIME_FORMAT);
            }
            return "";
        }

        public static string GetStatusFromFile(FilInfo fileRec)
        {
            if (fileRec.Filnavn.Contains("/lagret/"))
                return "Lagret";
            if (fileRec.Filnavn.Contains("/sendt/"))
                return "Sendt";
            if (fileRec.Filnavn.Contains("/mottatt/"))
                return "Mottatt";
            return "???";
        }

        public static List<PrefilledValue> ReadXMLFile(XmlSchema schema, IConfiguration config, string filename, bool editMode)
        {
            var schemaElement = XsdUtils.GetRootElement(schema)
                ?? throw new Exception($"Schema med nmsp '{schema?.TargetNamespace ?? "<ukjent>"}' ikke funnet");

            List<PrefilledValue> result = [];
            var xmlfile = BlobShareReaderWriter.GetFileStreamFromBlob(config, filename).Result;
            var doc = new XmlDocument();
            doc.Load(xmlfile);
            ReadElement(doc, null, "", schemaElement, result);
            return result;
        }
        public static void ReadXMLFile(XmlSchema schema, IConfiguration config, string filename, ref Dictionary<string, string> queryParams)
        {
            var fileContents = ReadXMLFile(schema, config, filename, false);
            foreach (var rec in fileContents)
            {
                if (!queryParams.ContainsKey(rec.Xpath))
                    queryParams.Add(rec.Xpath, rec.Value);
            }
        }


        public static List<XmlElement> GetChildrenOfName(XmlElement? element, string name)
        {
            // 
            List<XmlElement> result = [];
            if (element != null)
            {
                foreach (XmlElement child in element.ChildNodes)
                {
                    if (child.LocalName == name)
                        result.Add(child);
                }
            }
            return result;
        }

        public static void ReadElement(XmlDocument xmlDoc, XmlElement? parent, string xmlPath, XmlSchemaAnnotated elementType, List<PrefilledValue> values, int index = -1)
        {

            if (XsdUtils.GetIsRepeating(elementType) && index < 0) // repeterende elementer
            {
                string nmsp = XsdUtils.GetQualifiedNamespace(elementType);
                string elementName = XsdUtils.GetName(elementType);
                var elements = GetChildrenOfName(parent, elementName);

                for (int i = 0; i < elements.Count; i++)
                {
                    ReadElement(xmlDoc, parent, xmlPath, elementType, values, i + 1);
                }
            }

            else
            {
                string indexPart = "";
                if (index > 0)
                {
                    if (XsdUtils.HasUniqueConstraint(elementType))
                    {
                        var enumType = XsdUtils.GetIterateTypeDefinition(elementType)
                            ?? throw new Exception($"Finner ikke enumType '{XsdUtils.GetName(elementType)}'");

                        var kodeliste = XsdUtils.GetKodeliste(enumType)!;
                        if (kodeliste?.koder?.Length < index)
                            throw new Exception($"Finner ikke kode i kodeliste for enumType '{enumType.Name}'");
                        indexPart = $":{kodeliste!.koder![index - 1].verdi!.Replace(".", "-")}";
                    }
                    else
                        indexPart = $":{index}";
                }
                var newPath = (xmlPath.TrimEnd('.') + "." + XsdUtils.GetName(elementType)).TrimStart('.') + indexPart;
                if (elementType is XmlSchemaChoice choiceElement)
                {
                    foreach (XmlSchemaElement item in choiceElement.Items)
                    {
                        ReadElement(xmlDoc, parent, xmlPath, item, values);
                    }
                }

                else
                {
                    string nmsp = XsdUtils.GetQualifiedNamespace(elementType);
                    string elementname = XsdUtils.GetName(elementType);
                    XmlElement? element;

                    if (parent == null) // rotelement
                    {
                        element = xmlDoc.DocumentElement;
                    }
                    else if (index > 0)
                    {
                        var elements = GetChildrenOfName(parent, elementname);
                        element = elements[index - 1];
                    }
                    else
                    {
                        element = parent[elementname, nmsp];

                    }

                    if (XsdUtils.GetComplexType(elementType) != null && element != null)
                    {
                        var complexType = XsdUtils.GetComplexType(elementType);
                        foreach (var child in XsdUtils.GetXsdChildren(complexType!))
                        {

                            ReadElement(xmlDoc, element, newPath, child, values);
                        }
                    }
                    else if (element != null) // simple types
                    {
                        var text = element.InnerText;
                        if (text != "") // if element has value in xmldoc
                        {
                            values.Add(new(newPath, text, true, false));
                        }
                    }
                }
            }

        }

        public static void DeleteFile(IConfiguration config, string fileName)
        {
            if (string.IsNullOrEmpty(config["StorageAccountConnectString"]) && string.IsNullOrEmpty(config["StorageAccountClientId"])) // hvis ingen storage account emulator, burk lokal filsystem
            {
                File.Delete(fileName);
            }
            else
            {
                BlobShareReaderWriter.DeleteFile(config, fileName);
            }

        }

        internal static object CompareMeldingOrder(Dictionary<string, string> hode1, Dictionary<string, string> hode2)
        {
            var tid1 = hode1["mld:SendtTidspunkt"];
            var tid2 = hode2["mld:SendtTidspunkt"];
            return tid1.CompareTo(tid2);
        }
    }
}
