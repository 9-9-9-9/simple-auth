using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SimpleAuth.Client.AspNetCore.Attributes;
using WebMvc21Playground.Models;

namespace WebMvc21Playground.Controllers
{
    public class HomeController : Controller
    {
        [SaModule("x")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("abt")]
        [SaModule("x")]
        public async Task<IActionResult> About()
        {
            await Task.CompletedTask;

            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}