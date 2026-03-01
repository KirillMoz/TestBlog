using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestBlog.Models;
using TestBlog.Services;
using TestBlog.ViewModels;

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
        public IActionResult Contact(ContactViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Здесь можно добавить логику отправки email
            _logger.LogInformation("Отправлена форма обратной связи от {Name} ({Email})", model.Name, model.Email);
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
            var model = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            };
            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public new IActionResult StatusCode([FromQuery] int code)
        {
            ViewBag.StatusCode = code;
            return View();
        }
    }
}