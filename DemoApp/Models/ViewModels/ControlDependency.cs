namespace DemoApp.Models.ViewModels
{
    public class ControlEnabler(string enabledControlId, string controlId, string controlValue, bool isInverted)
    {
        public string EnabledControlId => enabledControlId;
        public string ControlId => controlId;
        public string ControlValue => controlValue;
        public bool IsInverted => isInverted;

    }
}
