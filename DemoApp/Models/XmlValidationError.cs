using System.Xml.Schema;

namespace DemoApp.Models;

public enum XmlValidationErrorType
{
    NoError = 0,
    MissingValue = 1,
    TooShortValue = 2,
    TooLongValue = 3,
    IncorrectValue = 4,
    CountOutOfRange = 5,
    PatternMatchError = 6,
    StandardXmlValidationError = 7
}

public class XmlValidationError(XmlSchemaObject? element, XmlValidationErrorType errorType, string errorText)
{
    public XmlSchemaObject? Element { get; set; } = element;
    public XmlValidationErrorType ErrorType { get; set; } = errorType;
    public string ErrorText { get; set; } = errorText;
}
