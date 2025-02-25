using System.Xml.Schema;

namespace DemoApp.Models.Test;

public static class XsdTester
{
    public static List<string> TestJsonTekster()
    {
        /*      
                XsdUtils.Tekster.missing.Clear();
                XsdUtils.Tekster.used.Clear();
                XsdUtils.Tekster.wrongName.Clear();
        */
        List<string> result = [];
        foreach (var item in XsdUtils.Tekster.henvisning.skjemaelement)
        {
            string itemValue = $"{item.id} - {item.navn}";
            if (XsdUtils.Tekster.used.IndexOf(itemValue) < 0)
                result.Add(itemValue);
        }
        return result;
    }

    public static List<string> TestKodelister()
    {
        List<string> result = [];
        foreach (var schema in XsdUtils.XsdSchemas)
        {
            var types = schema.SchemaTypes;
            foreach (var typ in types.Values)
            {
                if (typ is XmlSchemaAnnotated schType)
                {
                    var simpleType = XsdUtils.GetSimpleType(schType);
                    if (XsdUtils.GetIsEnumType(simpleType))
                    {
                        string name = XsdUtils.GetName(simpleType);
                        var enumValues = XsdUtils.GetEnumValues(schType);
                        var kodeliste = XsdUtils.GetKodeliste(schType);
                        string kodelisteId = schType.Id ?? $"<mangler, navn = {XsdUtils.GetName(schType)}>";
                        if (kodeliste == null)
                        {
                            result.Add($"Mangler kodeliste med id={kodelisteId}");
                        }
                        else
                        {
                            if (name != kodeliste.navn)
                            {
                                // kan være arvet type (restricted)
                                var baseType = XsdUtils.GetSimpleType(simpleType.BaseXmlSchemaType);
                                if (baseType == null || baseType.Name != kodeliste.navn)
                                    result.Add($"kodeliste med id={kodelisteId} har feil navn");
                            }
                            var allEnumValues = enumValues.Select(ev => ev.Key).ToList();
                            var allKodelisteValues = kodeliste.koder!.Select(k => k.verdi).ToList();
                            foreach (var key in allEnumValues)
                            {
                                int index = allKodelisteValues.IndexOf(key);
                                if (index < 0)
                                    result.Add($"kodeliste med id={kodelisteId} mangler verdi '{key}'");
                                else
                                    allKodelisteValues.RemoveAt(index);
                                if (allKodelisteValues.Contains(key))
                                    result.Add($"kodeliste med id={kodelisteId} har duplikatverdi '{key}'");
                            }
                            foreach (var key in allKodelisteValues)
                            {
                                result.Add($"kodeliste med id={kodelisteId} inneholder ikke-eksisterende enumverdi='{key}'");
                            }
                        }
                    }
                }
            }
        }
        return result;
    }

    private static void testType(XmlSchema schema, XmlSchemaAnnotated schType, List<string> usedIds, List<string> duplicates)
    {
        if (!(XsdUtils.GetQualifiedNamespace(schType) == schema.TargetNamespace))
            return;
        var parent = XsdUtils.GetParent(schType);
        var parentName = (parent != null) ? XsdUtils.GetName(parent) : "";
        string value = schType.Id + " => " + parentName + "/" + XsdUtils.GetName(schType);
        if (usedIds.Contains(value))
            duplicates.Add(value);
        else
            usedIds.Add(value);
        if (schType is XmlSchemaComplexType complexType)
        {
            foreach (var child in XsdUtils.GetXsdChildren(complexType))
            {
                testType(schema, child, usedIds, duplicates);
            }
        }
    }

    public static List<string> TestIder()
    {
        List<string> result = [];
        List<string> usedIds = [];
        foreach (var schema in XsdUtils.XsdSchemas)
        {
            foreach (var objType in schema.SchemaTypes.Values)
            {
                if (objType is XmlSchemaAnnotated schType)
                    testType(schema, schType, usedIds, result);
            }
        }
        return result;
    }

}
