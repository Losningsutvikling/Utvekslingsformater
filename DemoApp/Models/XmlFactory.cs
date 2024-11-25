using DemoApp.Models.Fagsystem;
using System.Xml;
using System.Xml.Schema;

namespace DemoApp.Models
{
    public static class XmlFactory
    {

        public static bool CreateXml(IConfiguration config, IWebHostEnvironment env, Dictionary<string, string> qp, string fileName)
        {
            Dictionary<string, string> ns = [];

            string selectedProtocolName = Utils.GetRequestValue(qp, Konstanter.SelectedProtocol);
            var meldingsProtokoll = XsdUtils.MeldingsprotokollVersjoner.FirstOrDefault(v => v.Version == selectedProtocolName);
            var skjemaRotElementName = Utils.GetRequestValue(qp, Konstanter.SelectedSkjema);
            var schemaElement = XsdUtils.FindByXPath(meldingsProtokoll!.TargetNamespace!, skjemaRotElementName);
            var xsdSchema = XsdUtils.GetSchema(schemaElement);


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
                if (schemaElement.SourceUri.Contains(file.Name))
                {
                    schemaLoc = file.Name;
                };
            }
            schemaSet.Compile();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Schemas.Add(schemaSet);
            XmlNode docNode = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            xmlDoc.AppendChild(docNode);

            Dictionary<string, string> rootElementAttributes = [];

            foreach (XmlSchema sch in schemaSet.Schemas())
            {
                foreach (var nms2 in sch.Namespaces.ToArray().ToList())
                {
                    if (nms2.Name != "" && nms2.Namespace.StartsWith("https://Bufdir.no") && !ns.ContainsKey(nms2.Namespace))
                    {
                        ns.Add(nms2.Namespace, nms2.Name);
                        rootElementAttributes["xmlns:" + nms2.Name] = nms2.Namespace;
                    }
                }
            }

            ns.Add(schemaElement.QualifiedName.Namespace, "melding");

            rootElementAttributes["xmlns"] = schemaElement.QualifiedName.Namespace;
            rootElementAttributes["xmlns:xsi"] = "http://www.w3.org/2001/XMLSchema-instance";

            WriteElement(xmlDoc, null, "", schemaElement, ns, qp);

            var rootDocumentElement = xmlDoc.ChildNodes[1] as XmlElement;


            foreach (var attr in rootElementAttributes)
            {
                rootDocumentElement.SetAttribute(attr.Key, attr.Value);
            }

            // schemalocation må legges til etter rotelement har xsi namespace for kunne få xsi prefix
            rootDocumentElement.SetAttribute("schemaLocation", "http://www.w3.org/2001/XMLSchema-instance", xsdSchema.TargetNamespace + " " + schemaLoc);

            BlobShareReaderWriter.SaveFile(config, fileName, xmlDoc);

            ValidateXml(fileName, config, env);

            return true;
        }


        public static bool WriteElement(XmlDocument xmlDoc, XmlElement? parent, string xmlPath, XmlSchemaAnnotated elementType,
                                        Dictionary<string, string> ns, Dictionary<string, string> qp, int index = -1)
        {
            bool result = false;
            if (XsdUtils.GetIsRepeating(elementType) && index < 0)
            {
                for (int i = 1; i < 50; i++)
                {
                    if (WriteElement(xmlDoc, parent, xmlPath, elementType, ns, qp, i))
                        result = true;
                }
            }
            else
            {
                string indexPart = (index > 0) ? $":{index}" : "";
                var newPath = (xmlPath.TrimEnd('.') + "." + XsdUtils.GetName(elementType)).TrimStart('.') + indexPart;
                if (elementType is XmlSchemaChoice choiceElement)
                {
                    if (qp.TryGetValue(xmlPath + "._CHOICE", out string chosen))
                    {
                        foreach (XmlSchemaElement item in choiceElement.Items)
                        {
                            if (item.Name == chosen)
                            {
                                if (WriteElement(xmlDoc, parent, newPath, item, ns, qp))
                                    result = true;
                            }
                        }
                    }
                }
                else
                {
                    string nmsp = XsdUtils.GetQualifiedNamespace(elementType);
                    var elementPrefix = ns[nmsp];
                    XmlElement element = xmlDoc.CreateElement(elementPrefix, XsdUtils.GetName(elementType), nmsp);

                    if (XsdUtils.GetComplexType(elementType) != null)
                    {
                        var complexType = XsdUtils.GetComplexType(elementType);
                        foreach (var child in XsdUtils.GetXsdChildren(complexType!))
                        {
                            if (WriteElement(xmlDoc, element, newPath, child, ns, qp))
                                result = true;
                        }
                    }
                    else // is SimpleType
                    {
                        string input;

                        if (XsdUtils.GetControlNameForProperty(elementType) == "Checkbox" && qp.ContainsKey($"{newPath}_FALSE")) // && !qp.ContainsKey(newPath)
                        {
                            element.InnerText = "false";
                            result = true;
                        }

                        else if (qp.ContainsKey(newPath))
                        {
                            input = qp[newPath];
                            if (input != "")
                            {
                                element.InnerText = input == "on" ? "true" : input;
                                qp.Remove(newPath);
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

        public static void ValidateXml(string xmlDoc, IConfiguration config, IWebHostEnvironment env)
        {

            string configPath = XsdUtils.GetDefinitionsDirectory(config);
            string DefsPath = (configPath.StartsWith("..")) ? configPath : Path.Combine(env.ContentRootPath, configPath);
            var files = new DirectoryInfo(DefsPath).GetFiles(@"*.xsd", SearchOption.TopDirectoryOnly/*AllDirectories*/);
            XmlSchemaSet schemaSet = new();
            foreach (var file in files)
            {
                using var reader = XmlReader.Create(file.FullName);
                XmlSchema schema = XmlSchema.Read(reader, null)
                        ?? throw new Exception($"Kan ikke laste {file.Name}");
                schemaSet.Add(schema);
            }
            schemaSet.Compile();

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.Schemas.Add(schemaSet);
            settings.Schemas.Compile();
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationEventHandler += new ValidationEventHandler(ErrorHandler);

            var xmlfile = BlobShareReaderWriter.GetFileStreamFromBlob(config, xmlDoc).Result;

            Console.WriteLine("validation result of file: ");
            XmlReader xReader = XmlReader.Create(xmlfile, settings);
            while (xReader.Read()) { }
        }

        public static void ErrorHandler(object sender, ValidationEventArgs args)
        {
            if (args.Severity == XmlSeverityType.Warning)
            {
                Console.Write("WARNING: ");
                Console.WriteLine(args.Message);
            }
            else if (args.Severity == XmlSeverityType.Error)
            {
                Console.Write("ERROR: ");
                Console.WriteLine(args.Message);
            }
        }

        public static Dictionary<string, Dictionary<string, string>> ReadXMlElement(IConfiguration config, string folder)
        {
            var fileInfoList = new Dictionary<string, Dictionary<string, string>>(); // dict av dict med filnavn som key
            var files = BlobShareReaderWriter.GetAllFiles(config, folder);

            foreach (var file in files)
            {
                var fileInfo = new Dictionary<string, string>();
                XmlReader reader;

                if (string.IsNullOrEmpty(config["StorageAccountConnectString"]) && string.IsNullOrEmpty(config["StorageAccountClientId"]))
                {
                    reader = XmlReader.Create(file);

                }
                else
                {
                    var xmlfile = BlobShareReaderWriter.GetFileStreamFromBlob(config, file).Result;
                    reader = XmlReader.Create(xmlfile);
                }

                while (reader.Read())
                {
                    if (reader.IsStartElement("mld:Meldingshode"))
                    {
                        while (reader.Read())
                        {
                            if (reader.Name == "mld:Meldingshode")
                            {
                                break;
                            }
                            var text = reader.ReadString();
                            if (!reader.IsStartElement() && text != "")
                            {
                                fileInfo[reader.Name] = text;
                            }
                        }
                        break;
                    }
                }
                fileInfoList.Add(file, fileInfo);
            }
            return fileInfoList;
        }



        public static List<PrefilledValue> ReadXMLFile(Dictionary<string, string> qp, IConfiguration config, string filename, string root, bool editMode)
        {

            string selectedProtocolName = Utils.GetRequestValue(qp, Konstanter.SelectedProtocol);
            var meldingsProtokoll = XsdUtils.MeldingsprotokollVersjoner.FirstOrDefault(v => v.Version == selectedProtocolName);
            var skjemaRotElementName = Utils.GetRequestValue(qp, Konstanter.SelectedSkjema);
            var schemaElement = XsdUtils.FindByXPath(meldingsProtokoll!.TargetNamespace!, skjemaRotElementName);
            var xsdSchema = XsdUtils.GetSchema(schemaElement);


            List<PrefilledValue> result = [];
            var xmlfile = BlobShareReaderWriter.GetFileStreamFromBlob(config, filename).Result;
            var doc = new XmlDocument();
            doc.Load(xmlfile);
            var rootelem = doc.ChildNodes[1] as XmlElement;
            ReadElement(doc, null, "", schemaElement, result);

            return result;
        }

        public static List<XmlElement> GetChildrenOfName(XmlElement element, string name)
        {
            List<XmlElement> result = [];
            foreach (XmlElement child in element.ChildNodes)
            {
                if (child.LocalName == name)
                    result.Add(child);
            }
            return result;
        }

        public static void ReadElement(XmlDocument xmlDoc, XmlElement parent, string xmlPath, XmlSchemaAnnotated elementType, List<PrefilledValue> values, int index = -1)
        {

            if (XsdUtils.GetIsRepeating(elementType) && index < 0)
            {
                string nmsp = XsdUtils.GetQualifiedNamespace(elementType);
                string elementname = XsdUtils.GetName(elementType);
                var elements = GetChildrenOfName(parent, elementname);
                for (int i = 0; i < elements.Count; i++)
                {
                    ReadElement(xmlDoc, parent, xmlPath, elementType, values, i + 1);
                }
            }

            else
            {
                string indexPart = (index > 0) ? $":{index}" : "";
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
                    XmlElement element;

                    if (parent == null)
                    {
                        element = xmlDoc[XsdUtils.GetName(elementType), nmsp];
                    }
                    else if (index > 0)
                    {
                        var elements = GetChildrenOfName(parent, elementname);
                        element = elements[index-1];
                    }
                    else
                    {
                        element = parent[XsdUtils.GetName(elementType), nmsp];

                    }

                    if (XsdUtils.GetComplexType(elementType) != null && element != null)
                    {

                        var complexType = XsdUtils.GetComplexType(elementType);
                        foreach (var child in XsdUtils.GetXsdChildren(complexType!))
                        {

                            ReadElement(xmlDoc, element, newPath, child, values);
                        }
                    }
                    else if (element != null)
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
    }
}
