using Microsoft.AspNetCore.Mvc;
using analizfutbol.Services;

namespace analizfutbol.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMongoDBService _mongoDBService;

        public HomeController(IMongoDBService mongoDBService)
        {
            _mongoDBService = mongoDBService;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
} 