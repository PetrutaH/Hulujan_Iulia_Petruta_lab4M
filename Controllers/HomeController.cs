using System.Diagnostics;
using Hulujan_Iulia_Petruta_lab4M.Models;
using Microsoft.AspNetCore.Mvc;

namespace Hulujan_Iulia_Petruta_lab4M.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
