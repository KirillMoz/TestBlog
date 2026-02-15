using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TestBlog.Services;

namespace TestBlog.Controllers
{
    public class ArticleController : Controller
    {
        private readonly IArticleService _articleService;
        private readonly ITagService _tagService;
        private readonly IUserService _userService;

        public ArticleController(
            IArticleService articleService,
            ITagService tagService,
            IUserService userService)
        {
            _articleService = articleService;
            _tagService = tagService;
            _userService = userService;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var articles = await _articleService.GetPublishedArticlesAsync();
            return View(articles);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var article = await _articleService.GetArticleByIdAsync(id);
            if (article == null)
                return NotFound();

            await _articleService.IncrementViewCountAsync(id);
            return View(article);
        }

        [Authorize]
        public async Task<IActionResult> Create()
        {
            ViewBag.Tags = await _tagService.GetAllTagsAsync();
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Models.Article article, int[] selectedTags)
        {
            if (ModelState.IsValid)
            {
                // Получаем имя пользователя с проверкой
                var username = User?.Identity?.Name;

                if (string.IsNullOrWhiteSpace(username))
                {
                    ModelState.AddModelError("", "User is not authenticated");
                    ViewBag.Tags = await _tagService.GetAllTagsAsync();
                    return View(article);
                }

                // Получаем пользователя из БД
                var user = await _userService.GetUserByUsernameAsync(username);

                if (user == null)
                {
                    ModelState.AddModelError("", "User not found");
                    ViewBag.Tags = await _tagService.GetAllTagsAsync();
                    return View(article);
                }

                article.AuthorId = user.Id;

                // Создаем статью
                var result = await _articleService.CreateArticleAsync(
                    article,
                    selectedTags?.ToList() ?? new List<int>()
                );

                if (result)
                {
                    TempData["Success"] = "Article created successfully!";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", "Failed to create article");
            }

            // В случае ошибки загружаем теги и возвращаем форму
            ViewBag.Tags = await _tagService.GetAllTagsAsync();
            return View(article);
        }

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Admin()
        {
            var articles = await _articleService.GetAllArticlesAsync();
            return View(articles);
        }

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Publish(int id)
        {
            await _articleService.PublishArticleAsync(id);
            return RedirectToAction(nameof(Admin));
        }

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Unpublish(int id)
        {
            await _articleService.UnpublishArticleAsync(id);
            return RedirectToAction(nameof(Admin));
        }
    }
}