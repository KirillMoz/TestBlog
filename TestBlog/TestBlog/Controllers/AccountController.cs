using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TestBlog.Services.Interfaces;
using TestBlog.ViewModels.Account;

namespace TestBlog.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IUserService userService, ILogger<AccountController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var isAuthenticated = await _userService.AuthenticateAsync(model.Username, model.Password);

            if (!isAuthenticated)
            {
                ModelState.AddModelError("", "Неверное имя пользователя или пароль");
                return View(model);
            }

            var user = await _userService.GetUserByUsernameAsync(model.Username);

            if (user == null || !user.IsActive)
            {
                ModelState.AddModelError("", "Аккаунт деактивирован");
                return View(model);
            }

            var roles = await _userService.GetUserRolesAsync(user.Id);
            var roleNames = roles.Select(r => r.Name).ToList();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
            };

            foreach (var roleName in roleNames)
            {
                if (!string.IsNullOrEmpty(roleName))
                    claims.Add(new Claim(ClaimTypes.Role, roleName));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                claimsPrincipal,
                new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                });

            user.LastLoginDate = DateTime.Now;
            await _userService.UpdateUserAsync(user);

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
            _logger.LogInformation($"=== ПОПЫТКА РЕГИСТРАЦИИ ===");
            _logger.LogInformation($"Username: '{model.Username}', Email: '{model.Email}'");

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
                    _logger.LogInformation($"✅ РЕГИСТРАЦИЯ УСПЕШНА: {model.Username}");

                    // Автоматический вход после регистрации
                    await Login(new LoginViewModel
                    {
                        Username = model.Username,
                        Password = model.Password,
                        RememberMe = false
                    });

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    _logger.LogWarning($"❌ РЕГИСТРАЦИЯ НЕ УДАЛАСЬ: {model.Username}");
                    ModelState.AddModelError("", "Имя пользователя или email уже существует");
                }
            }
            else
            {
                _logger.LogWarning("Модель не валидна");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogWarning($"Ошибка валидации: {error.ErrorMessage}");
                }
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
        public async Task<IActionResult> Profile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return RedirectToAction("Login");
            }

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            ViewBag.Roles = roles;
            return View(user);
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return RedirectToAction("Login");
            }

            var result = await _userService.ChangePasswordAsync(userId, model.OldPassword, model.NewPassword);

            if (result)
            {
                TempData["SuccessMessage"] = "Пароль успешно изменен";
                return RedirectToAction("Profile");
            }

            ModelState.AddModelError("", "Неверный текущий пароль");
            return View(model);
        }
    }
}