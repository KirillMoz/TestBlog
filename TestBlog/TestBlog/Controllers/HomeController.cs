using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestBlog.Services;

namespace TestBlog.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IArticleService _articleService;
        private readonly ITagService _tagService;

        public HomeController(
            ILogger<HomeController> logger,
            IArticleService articleService,
            ITagService tagService)
        {
            _logger = logger;
            _articleService = articleService;
            _tagService = tagService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult About()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult Contact(string name, string email, string message)
        {
            // Здесь можно добавить логику отправки email
            TempData["SuccessMessage"] = "Сообщение успешно отправлено! Мы свяжемся с вами в ближайшее время.";
            return RedirectToAction("Contact");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }

        public IActionResult NotFound404()
        {
            return NotFound();
        }
    }
}