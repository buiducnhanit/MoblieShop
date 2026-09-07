using Microsoft.AspNetCore.Mvc;
using MoblieShop.Data;
using MoblieShop.Models;

namespace MoblieShop.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
