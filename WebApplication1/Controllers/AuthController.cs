using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Helpers;
using WebApplication1.Models;
using WebApplication1.ViewModel;

namespace WebApplication1.Controllers
{
    public class AuthController : Controller
    {
        private readonly FastFoodContext _db;
        private const string CURRENT_USER_SESSION_KEY = "CURRENT_USER";

        public AuthController(FastFoodContext db)
        {
            _db = db;
        }

        private UserSessionVm? CurrentUser()
            => HttpContext.Session.Get<UserSessionVm>(CURRENT_USER_SESSION_KEY);

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            var me = CurrentUser();
            if (me != null)
            {
                if (!string.IsNullOrWhiteSpace(me.Role) &&
                    me.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "PrivateSite" });
                }

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _db.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(u => u.UserName == model.Username && u.IsActive);

            if (user == null)
            {
                ModelState.AddModelError("", "Tài khoản không tồn tại hoặc đã bị khóa.");
                return View(model);
            }

            if (!BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Mật khẩu không đúng.");
                return View(model);
            }

            var sessionUser = new UserSessionVm
            {
                UserId = user.UserId,
                UserName = user.UserName,
                FullName = user.FullName,
                Role = user.Role
            };
            HttpContext.Session.Set(CURRENT_USER_SESSION_KEY, sessionUser);

            if (!string.IsNullOrWhiteSpace(user.Role) &&
                user.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "PrivateSite" });
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            var me = CurrentUser();
            if (me != null)
            {
                if (!string.IsNullOrWhiteSpace(me.Role) &&
                    me.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                    return RedirectToAction("Index", "Dashboard", new { area = "PrivateSite" });

                return RedirectToAction("Index", "Home");
            }

            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var existUser = await _db.Users.AsNoTracking()
                .AnyAsync(u => u.UserName == model.Username);
            if (existUser)
            {
                ModelState.AddModelError(nameof(model.Username), "Tên đăng nhập đã tồn tại.");
                return View(model);
            }

            if (!string.IsNullOrWhiteSpace(model.Email))
            {
                var existEmail = await _db.Users.AsNoTracking()
                    .AnyAsync(u => u.Email == model.Email);
                if (existEmail)
                {
                    ModelState.AddModelError(nameof(model.Email), "Email đã được sử dụng.");
                    return View(model);
                }
            }

            var user = new User
            {
                UserName = model.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                FullName = model.FullName,
                Email = model.Email,
                Phone = model.Phone,
                Role = "Customer",
                IsActive = true
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var sessionUser = new UserSessionVm
            {
                UserId = user.UserId,
                UserName = user.UserName,
                FullName = user.FullName,
                Role = user.Role
            };
            HttpContext.Session.Set(CURRENT_USER_SESSION_KEY, sessionUser);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove(CURRENT_USER_SESSION_KEY);
            return RedirectToAction("Login");
        }
    }
}
