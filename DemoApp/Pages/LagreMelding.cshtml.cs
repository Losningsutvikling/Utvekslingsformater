using DemoApp.Models;
using DemoApp.Models.Simulator;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DemoApp.Pages
{
    public class LagreMeldingModel(IConfiguration config, IWebHostEnvironment env) : PageModel
    {
        public void OnPost()
        {
            Init();
        }

        public void OnGet()
        {
            Init();
        }

        public void Init()
        {
            Utils.GetRequestParams(Request, out QueryParams);
            string existingFile = Utils.GetRequestValue(QueryParams, Konstanter.XmlFil);
            bool fileExists = existingFile != "";
            string schemaNmsp = "";
            if (fileExists)
            {
                var info = XmlFactory.ReadFileMeldingshode(config, existingFile);
                schemaNmsp = info.Meldingshode.MeldingstypeNmsp;
            }
            else
                schemaNmsp = Utils.GetRequestValue(QueryParams, Konstanter.SelectedSkjema);
            var schema = XsdUtils.GetSchema(schemaNmsp);
            var rootElement = XsdUtils.GetRootElement(schema);
            if (QueryParams.ContainsKey("actionParam"))
            {
                Action = Utils.GetRequestValue(QueryParams, "actionParam");
            }
            if (!Utils.ExistsRequestValue(QueryParams, "Redigert"))
            {
                XmlFactory.ReadXMLFile(schema, config, existingFile, ref QueryParams);
                //XmlFactory.XReadXMLFile(schema, config, existingFile, false);// ref QueryParams);
                QueryParams[Konstanter.SelectedSkjema] = schemaNmsp;
            }
            var dateTime = TestdataGenerator.Update_SendtTidspunkt(QueryParams, rootElement!.Name!);
            string fileName = XmlFactory.GenerateFileName(schemaNmsp, QueryParams, Action, dateTime);
            FileName = XmlFactory.WriteXml(config, env, true, QueryParams, fileName, Errors);
            if (existingFile != "" && !existingFile.Contains("/sendt/"))
                XmlFactory.DeleteFile(config, existingFile);
            Action = (FileName.Contains("/sendt/")) ? "Sendt" : "lagret";
        }

        public List<XmlValidationError> Errors { get; set; } = [];

        public Dictionary<string, string> QueryParams = [];

        public string FileName { get; set; } = "";

        public string Action { get; set; } = "";

    }
}
