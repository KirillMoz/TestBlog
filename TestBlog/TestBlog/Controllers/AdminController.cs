using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestBlog.Services;

namespace TestBlog.Controllers
{
    [Authorize(Roles = "Admin")] // Весь контроллер только для администраторов
    public class AdminController : Controller
    {
        private readonly IUserService _userService;
        private readonly IArticleService _articleService;

        public AdminController(
            IUserService userService,
            IArticleService articleService)
        {
            _userService = userService;
            _articleService = articleService;
        }

        // Этот метод доступен только пользователям с ролью Admin
        public async Task<IActionResult> Dashboard()
        {
            var users = await _userService.GetAllUsersAsync();
            var articles = await _articleService.GetAllArticlesAsync();

            ViewBag.UsersCount = users.Count();
            ViewBag.ArticlesCount = articles.Count();
            ViewBag.PublishedArticlesCount = articles.Count(a => a.IsPublished);

            return View();
        }

        // Этот метод доступен только администраторам
        public async Task<IActionResult> Users()
        {
            var users = await _userService.GetAllUsersAsync();
            return View(users);
        }

        // Этот метод доступен только администраторам
        public async Task<IActionResult> UserRoles(int userId)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            var userRoles = await _userService.GetUserRolesAsync(userId);
            var allRoles = await _userService.GetAllUsersAsync(); // Здесь нужно получить все роли

            ViewBag.User = user;
            ViewBag.UserRoles = userRoles;

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")] // Явное указание, что только для Admin
        public async Task<IActionResult> AddToRole(int userId, string roleName)
        {
            await _userService.AddUserToRoleAsync(userId, roleName);
            return RedirectToAction(nameof(UserRoles), new { userId });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveFromRole(int userId, string roleName)
        {
            await _userService.RemoveUserFromRoleAsync(userId, roleName);
            return RedirectToAction(nameof(UserRoles), new { userId });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeactivateUser(int userId)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user != null && user.Username != "admin")
            {
                user.IsActive = false;
                await _userService.UpdateUserAsync(user);
            }
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ActivateUser(int userId)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user != null)
            {
                user.IsActive = true;
                await _userService.UpdateUserAsync(user);
            }
            return RedirectToAction(nameof(Users));
        }

        // ⚠️ ПРИМЕР: Отдельный метод, заблокированный для администратора
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult SecretAdminPanel()
        {
            return Content("This is secret admin panel. Only administrators can see this!");
        }

        // ⚠️ ПРИМЕР: Метод доступен только Admin ИЛИ Moderator
        [HttpGet]
        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult ModerationPanel()
        {
            return Content("This is moderation panel. Admins and Moderators can see this!");
        }
    }
}