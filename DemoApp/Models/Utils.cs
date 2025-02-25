using DemoApp.Models.ViewModels;

namespace DemoApp.Models
{
    public static class Utils
    {
        public static void GetRequestParams(HttpRequest req, out Dictionary<string, string> queryParams)
        {
            queryParams = [];
            foreach (var kvp in req.Query)
            {
                queryParams[kvp.Key] = kvp!.Value.ToString();
            }
            if (req.HasFormContentType)
            {
                foreach (var kvp in req.Form)
                {
                    queryParams[kvp.Key] = kvp!.Value.ToString();
                }
            }
        }

        public static string GetRequestValue(Dictionary<string, string> RequestParams, string param)
        {
            if (RequestParams.TryGetValue(param, out var value))
                return value;
            return "";
        }
        public static bool ExistsRequestValue(Dictionary<string, string> RequestParams, string param)
        {
            return RequestParams.TryGetValue(param, out var _);
        }

        public static string GetRequestValue(HttpRequest req, string param)
        {
            GetRequestParams(req, out Dictionary<string, string> RequestParams);
            if (RequestParams == null)
                return "";
            return GetRequestValue(RequestParams, param);
        }

        /// <summary>
        /// Gets the value of the first key ending with the parameter 'partialKey'
        /// </summary>
        public static string GetRequestValuePartialKey(Dictionary<string, string> RequestParams, string partialKey)
        {
            foreach (var kvp in RequestParams)
            {
                if (kvp.Key.EndsWith(partialKey))
                    return kvp.Value;
            }
            return "";
        }

        public static string Reformat(string dateTime, string currentFormat, string newFormat)
        {
            if (DateTime.TryParseExact(dateTime, currentFormat, null, System.Globalization.DateTimeStyles.None, out DateTime result))
                return result.ToString(newFormat);
            return "";

        }



        public static string getMandatoryMarker(PropertyRendererModel model, bool disable = false, bool overrideModel = false)
        {
            bool returnMarker = overrideModel;
            if (!returnMarker)
            {
                if ((XsdUtils.IsSimpleType(model.Prop) && model.Mandatory) || XsdUtils.AllChoiceElementsSimpleAndMandatory(model.Prop))
                    returnMarker = true;
                else if (XsdUtils.ChoiceElementMandatory(model.Prop))
                    returnMarker = true;
            }
            string disableCssClass = (disable) ? "disabled" : "";
            return (returnMarker) ? $"<span class='obligatorisk_markor {disableCssClass}'>*</span>" : "";
        }

        private const int MIN_LENGDE_FOR_A_VISE_MIN_MARKOR = 10;

        public static string getMinLengthMarker(PropertyRendererModel model)
        {
            int minlength = XsdUtils.GetMinLength(model.Prop);
            return (minlength > MIN_LENGDE_FOR_A_VISE_MIN_MARKOR) ? $"<span class='minlength_markor'>minst {minlength} tegn</span>" : "";
        }
    }
}
