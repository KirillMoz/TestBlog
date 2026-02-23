using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestBlog.Services;
using TestBlog.ViewModels;

namespace TestBlog.Controllers
{
    public class TagController : Controller
    {
        private readonly ITagService _tagService;
        private readonly IArticleService _articleService;

        public TagController(ITagService tagService, IArticleService articleService)
        {
            _tagService = tagService;
            _articleService = articleService;
        }

        // GET: /Tag/Index
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var tags = await _tagService.GetAllTagsAsync();

            // Для каждого тега получаем количество статей
            var tagViewModels = new List<TagViewModel>();
            foreach (var tag in tags)
            {
                var articles = await _articleService.GetArticlesByTagAsync(tag.Id);
                tagViewModels.Add(new TagViewModel
                {
                    Id = tag.Id,
                    Name = tag.Name ?? string.Empty, // 👈 Добавляем проверку на null
                    Description = tag.Description,
                    ArticlesCount = articles.Count()
                });
            }

            return View(tagViewModels.OrderByDescending(t => t.ArticlesCount));
        }

        // GET: /Tag/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var tag = await _tagService.GetTagByIdAsync(id);
            if (tag == null)
                return NotFound();

            var articles = await _articleService.GetArticlesByTagAsync(id);

            ViewBag.TagName = tag.Name;
            return View(articles);
        }

        // GET: /Tag/Create (только для админов)
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            // 👈 ИСПРАВЛЕНО: Создаем модель с пустым Name
            var model = new TagViewModel
            {
                Name = string.Empty
            };
            return View(model);
        }

        // POST: /Tag/Create
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TagViewModel model)
        {
            if (ModelState.IsValid)
            {
                var tag = new Models.Tag
                {
                    Name = model.Name,
                    Description = model.Description
                };

                var result = await _tagService.CreateTagAsync(tag);
                if (result)
                {
                    TempData["SuccessMessage"] = "Тег успешно создан";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Ошибка при создании тега");
            }
            return View(model);
        }

        // GET: /Tag/Edit/5 (только для админов)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var tag = await _tagService.GetTagByIdAsync(id);
            if (tag == null)
                return NotFound();

            var model = new TagViewModel
            {
                Id = tag.Id,
                Name = tag.Name ?? string.Empty, // 👈 Добавляем проверку на null
                Description = tag.Description
            };

            return View(model);
        }

        // POST: /Tag/Edit/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TagViewModel model)
        {
            if (id != model.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                var tag = new Models.Tag
                {
                    Id = model.Id,
                    Name = model.Name,
                    Description = model.Description
                };

                var result = await _tagService.UpdateTagAsync(tag);
                if (result)
                {
                    TempData["SuccessMessage"] = "Тег успешно обновлен";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Ошибка при обновлении тега");
            }
            return View(model);
        }

        // POST: /Tag/Delete/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _tagService.DeleteTagAsync(id);
            if (result)
            {
                TempData["SuccessMessage"] = "Тег успешно удален";
            }
            else
            {
                TempData["ErrorMessage"] = "Ошибка при удалении тега";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}