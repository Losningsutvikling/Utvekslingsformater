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

        public static string GetRequestValue(HttpRequest req, string param)
        {
            GetRequestParams(req, out Dictionary<string, string> RequestParams);
            if (RequestParams == null)
                return "";
            return GetRequestValue(RequestParams, param);
        }

    }
}
