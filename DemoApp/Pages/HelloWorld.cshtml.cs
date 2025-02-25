using DemoApp.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DemoApp.Pages
{
    public class HelloWorldModel(IConfiguration config) : PageModel
    {
        public string message { get; set; } = "";
        public async void OnGet()
        {
            try
            {
                Task result = BlobShareReaderWriter.SaveFileContentsToBlob(config, "test.file", "Hello, world!");
                await result;
                message = "OK";
            }
            catch (Exception e)
            {
                message = e.Message + "\n" + e.StackTrace;
            }
        }
    }
}
