using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TestBlog.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using TestBlog.Services;
using TestBlog.Services.Interfaces;

namespace TestBlog.Controllers
{
    [Authorize]
    public class ArticleController : Controller
    {
        private readonly IArticleService _articleService;
        private readonly ITagService _tagService;
        private readonly IUserService _userService;
        private readonly ILogger<ArticleController> _logger;

        public ArticleController(
            IArticleService articleService,
            ITagService tagService,
            IUserService userService,
            ILogger<ArticleController> logger)
        {
            _articleService = articleService;
            _tagService = tagService;
            _userService = userService;
            _logger = logger;
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
            {
                _logger.LogWarning("Статья с ID {ArticleId} не найдена", id);
                return NotFound();
            }

            await _articleService.IncrementViewCountAsync(id);
            return View(article);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Tags = await _tagService.GetAllTagsAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Article article, int[] selectedTags)
        {
            // В ASP.NET Core HTML-код разрешен по умолчанию, если не использовать атрибут [HtmlEncode]
            // Для безопасности можно добавить проверку на разрешенные теги

            if (ModelState.IsValid)
            {
                var username = User.Identity?.Name;
                if (string.IsNullOrEmpty(username))
                    return RedirectToAction("Login", "Account");
                var user = await _userService.GetUserByUsernameAsync(username);
                
                if (user == null)
                    return RedirectToAction("Login", "Account");
                article.AuthorId = user.Id;

                var result = await _articleService.CreateArticleAsync(
                    article, selectedTags?.ToList() ?? new List<int>());

                if (result)
                {
                    _logger.LogInformation("Пользователь {User} создал статью '{Title}'", User.Identity?.Name, article.Title);
                    TempData["SuccessMessage"] = "Статья успешно создана!";
                    return RedirectToAction(nameof(MyArticles));
                }
                else
                {
                    _logger.LogWarning("Не удалось создать статью '{Title}' пользователем {User}", article.Title, User.Identity?.Name);
                    ModelState.AddModelError("", "Ошибка при создании статьи");
                }
            }

            ViewBag.Tags = await _tagService.GetAllTagsAsync();
            return View(article);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var article = await _articleService.GetArticleByIdAsync(id);
            if (article == null)
                return NotFound();

            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            var user = await _userService.GetUserByUsernameAsync(username);
            if (user == null)
                return RedirectToAction("Login", "Account");

            if (article.AuthorId != user.Id && !User.IsInRole("Admin") && !User.IsInRole("Moderator"))
            {
                _logger.LogWarning("Пользователь {User} попытался редактировать чужую статью ID {ArticleId}", User.Identity?.Name, id);
                return Forbid();
            }

            ViewBag.Tags = await _tagService.GetAllTagsAsync() ?? new List<Tag>();
            return View(article);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Article article, int[] selectedTags)
        {
            if (id != article.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                var result = await _articleService.UpdateArticleAsync(
                    article, selectedTags?.ToList() ?? new List<int>());

                if (result)
                {
                    _logger.LogInformation("Пользователь {User} обновил статью ID {ArticleId}", User.Identity?.Name, article.Id);
                    TempData["SuccessMessage"] = "Статья успешно обновлена!";
                    return RedirectToAction(nameof(Details), new { id = article.Id });
                }
                else
                {
                    _logger.LogWarning("Не удалось обновить статью ID {ArticleId}", article.Id);
                    ModelState.AddModelError("", "Ошибка при обновлении статьи");
                }
            }

            ViewBag.Tags = await _tagService.GetAllTagsAsync();
            return View(article);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var article = await _articleService.GetArticleByIdAsync(id);
            if (article == null)
                return NotFound();

            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            var user = await _userService.GetUserByUsernameAsync(username);
            if (user == null)
                return RedirectToAction("Login", "Account");

            if (article.AuthorId != user.Id && !User.IsInRole("Admin"))
            {
                _logger.LogWarning("Пользователь {User} попытался удалить чужую статью ID {ArticleId}", User.Identity?.Name, id);
                return Forbid();
            }

            return View(article);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _articleService.DeleteArticleAsync(id);
            _logger.LogInformation("Пользователь {User} удалил статью ID {ArticleId}", User.Identity?.Name, id);
            TempData["SuccessMessage"] = "Статья успешно удалена!";
            return RedirectToAction(nameof(MyArticles));
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
            _logger.LogInformation("Пользователь {User} опубликовал статью ID {ArticleId}", User.Identity?.Name, id);
            TempData["SuccessMessage"] = "Статья опубликована!";
            return RedirectToAction(nameof(Admin));
        }

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Unpublish(int id)
        {
            await _articleService.UnpublishArticleAsync(id);
            _logger.LogInformation("Пользователь {User} снял с публикации статью ID {ArticleId}", User.Identity?.Name, id);
            TempData["SuccessMessage"] = "Статья снята с публикации!";
            return RedirectToAction(nameof(Admin));
        }

        // Мои статьи
        [Authorize]
        public async Task<IActionResult> MyArticles()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            var user = await _userService.GetUserByUsernameAsync(username);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var articles = await _articleService.GetArticlesByAuthorAsync(user.Id);
            return View(articles);
        }

        // Статьи по тегу
        [AllowAnonymous]
        public async Task<IActionResult> ByTag(int tagId)
        {
            var tag = await _tagService.GetTagByIdAsync(tagId);
            if (tag == null)
                return NotFound();

            var articles = await _articleService.GetArticlesByTagAsync(tagId);

            ViewBag.TagName = tag.Name;
            return View(articles);
        }
    }
}