using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TestBlog.Services;

using TestBlog.ViewModels.Account;

namespace TestBlog.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;

        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// Аутентификация пользователя с сохранением ролей в клеймах
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // 1. Аутентификация пользователя
            var isAuthenticated = await _userService.AuthenticateAsync(model.Username, model.Password);

            if (!isAuthenticated)
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View(model);
            }

            // 2. Получаем пользователя
            var user = await _userService.GetUserByUsernameAsync(model.Username);

            if (user == null || !user.IsActive)
            {
                ModelState.AddModelError("", "Account is deactivated");
                return View(model);
            }

            // 3. Получаем роли пользователя
            var roles = await _userService.GetUserRolesAsync(user.Id);
            var roleNames = roles.Select(r => r.Name).ToList();

            // 4. СОЗДАЕМ КЛЕЙМЫ И СОХРАНЯЕМ РОЛИ
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("UserId", user.Id.ToString()),
                new Claim("RegistrationDate", user.RegistrationDate.ToString("yyyy-MM-dd")),
                new Claim("IsActive", user.IsActive.ToString())
            };

            // 5. ДОБАВЛЯЕМ КАЖДУЮ РОЛЬ КАК ОТДЕЛЬНЫЙ КЛЕЙМ
            foreach (var roleName in roleNames)
            {
                claims.Add(new Claim(ClaimTypes.Role, roleName));
                claims.Add(new Claim("UserRole", roleName)); // дополнительный клейм для удобства
            }

            // 6. Добавляем primary role (первая роль)
            if (roleNames.Any())
            {
                claims.Add(new Claim("PrimaryRole", roleNames.First()));
            }

            // 7. Обновляем дату последнего входа
            user.LastLoginDate = DateTime.Now;
            await _userService.UpdateUserAsync(user);

            // 8. Создаем ClaimsIdentity и ClaimsPrincipal
            var claimsIdentity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // 9. Выполняем вход
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                claimsPrincipal,
                new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                }
            );

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new Models.User
                {
                    Username = model.Username,
                    Email = model.Email
                };

                var result = await _userService.CreateUserAsync(user, model.Password);

                if (result)
                {
                    return RedirectToAction("Login");
                }

                ModelState.AddModelError("", "Username or email already exists.");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var user = await _userService.GetUserByIdAsync(userId);
            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            ViewBag.Roles = roles;
            return View(user);
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}