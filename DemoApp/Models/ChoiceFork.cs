using System.Xml.Schema;

namespace DemoApp.Models
{
    public class ChoiceFork
    {
        public ChoiceFork(XmlSchemaElement element, List<ChoiceFork>? choiceElements)
        {
            Element = element;
            ChoiceElements = choiceElements;
        }
        public XmlSchemaElement Element { get; set; }
        public List<ChoiceFork>? ChoiceElements { get; set; }

    }
}
