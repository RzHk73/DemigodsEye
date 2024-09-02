using CameraMoniter.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System.Buffers;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Text;

namespace CameraMoniter.Controllers
{
    [Controller]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private static string? _data;

        private static bool newImg = false;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            try
            {
                System.IO.File.AppendAllText($"Logs/{DateTime.Now.ToString("dd MM yyyy")}.log", $"Page viewed by -> {Request.Headers["X-Forwarded-For"]} - {Request.Headers["X_Real-IP"]}\n");
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.ToString());
            }

            return View(new ImageModel() { Base64Data = _data ?? "", fps = float.Parse(Environment.GetEnvironmentVariable("FPS") ?? "0.01666") });

        }

        [HttpPost]
        public async Task<ActionResult> SetImage()
        {
            Console.WriteLine("Received Packet");

            byte[] data = null;

            using(var reader = new StreamReader(Request.Body))
            using(var ms = new MemoryStream())
            {
                await reader.BaseStream.CopyToAsync(ms);
                data = ms.ToArray();
            }
            
            if (data == null) return NoContent();

            string filename = DateTime.Now.ToString("yyyy-MM-dd HHmm ss");
            System.IO.File.WriteAllBytes($"Images/{filename}.jpeg", data.ToArray());

            _data = Convert.ToBase64String(data);
            newImg = true;

            return Ok();
        }

        [HttpGet]
        public JsonResult GetImage()
        {
            bool wasNewImg = newImg;
            if (newImg)
            {
                wasNewImg = true;
                newImg = false;
            }

            return Json(new { data = _data,  newImg = wasNewImg });
        }
    }
}
