namespace DemoApp.Models.Simulator
{
    public class PrefilledValue(string xpath, string value, bool openToEdit, bool initiallyClosed = false)
    {
        public string Xpath { get; set; } = xpath;
        public string Value { get; set; } = value;
        public bool OpenToEdit { get; set; } = openToEdit;
        public bool InitiallyClosed { get; set; } = initiallyClosed;
        public override string ToString()
        {
            return $"{Xpath}={Value}";
        }
    }
}
