namespace DemoApp.Models.ViewModels
{
    public class ControlEnabler(string enabledControlId, string controlId, string controlValue)
    {
        public string EnabledControlId => enabledControlId;
        public string ControlId => controlId;
        public string ControlValue => controlValue;

    }
}
