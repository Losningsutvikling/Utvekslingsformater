using DemoApp.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DemoApp.Pages
{
    public class HelloWorldModel(IConfiguration config) : PageModel
    {
        public string message { get; set; } = "";
        public void OnGet()
        {
            try
            {
                BlobShareReaderWriter.SaveFileContents(config, "test.file", "Hello, world!");
                message = "OK";
            }
            catch (Exception e)
            {
                message = e.Message + "\n" + e.StackTrace;
            }
        }
    }
}
