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
                var username = User?.Identity?.Name;
                var user = await _userService.GetUserByUsernameAsync(username);
                article.AuthorId = user.Id;

                var result = await _articleService.CreateArticleAsync(
                    article, selectedTags?.ToList() ?? new List<int>());

                if (result)
                {
                    return RedirectToAction(nameof(Index));
                }
            }

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