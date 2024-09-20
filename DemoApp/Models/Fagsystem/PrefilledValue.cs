namespace DemoApp.Models.Fagsystem
{
    public class PrefilledValue(string xpath, string value, bool openToEdit)
    {
        public string Xpath { get; } = xpath;
        public string Value { get; } = value;
        public bool OpenToEdit { get; } = openToEdit;
    }
}
