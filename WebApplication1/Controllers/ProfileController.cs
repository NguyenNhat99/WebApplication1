using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Helpers; // SessionExtensions
using WebApplication1.Models;
using WebApplication1.ViewModel;

namespace WebApplication1.Controllers
{
    [CustomAuthorize(Roles = "Customer")]
    public class ProfileController : Controller
    {
        private readonly FastFoodContext _db;
        private const string CURRENT_USER_SESSION_KEY = "CURRENT_USER";

        public ProfileController(FastFoodContext db)
        {
            _db = db;
        }

        private UserSessionVm? CurrentUser()
            => HttpContext.Session.Get<UserSessionVm>(CURRENT_USER_SESSION_KEY);

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var me = CurrentUser();
            if (me == null)
                return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("Index", "Profile") });

            var user = await _db.Users.AsNoTracking()
                                      .FirstOrDefaultAsync(u => u.UserId == me.UserId);
            if (user == null) return NotFound();

            var vm = new UserProfileVM
            {
                UserId = user.UserId,
                Username = user.UserName,
                FullName = user.FullName ?? "",
                Email = user.Email ?? "",        // đảm bảo không null
                Phone = user.Phone
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UserProfileVM model)
        {
            var me = CurrentUser();
            if (me == null)
                return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("Index", "Profile") });

            // Tự validate nhẹ
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(model.FullName))
                errors.Add("Họ và tên không được để trống.");

            if (errors.Any())
            {
                // NẠP LẠI Email/Username từ DB để view không bị trống khi return View(model)
                var u0 = await _db.Users.AsNoTracking()
                                        .FirstOrDefaultAsync(u => u.UserId == me.UserId);
                if (u0 == null) return NotFound();

                model.Username = u0.UserName;
                model.Email = u0.Email ?? "";

                TempData["Error"] = string.Join("<br/>", errors);
                return View("Index", model);
            }

            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == me.UserId);
            if (user == null) return NotFound();

            // Cập nhật KHÔNG đụng tới Email (vì đang readonly/disabled)
            user.FullName = model.FullName.Trim();
            user.Phone = model.Phone?.Trim();

            await _db.SaveChangesAsync();

            // Cập nhật session hiển thị header
            me.FullName = user.FullName;
            HttpContext.Session.Set(CURRENT_USER_SESSION_KEY, me);

            TempData["Toast"] = "Đã lưu thông tin tài khoản.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordVM model)
        {
            var me = CurrentUser();
            if (me == null) return Unauthorized(new { success = false, message = "Hết phiên đăng nhập." });

            if (string.IsNullOrWhiteSpace(model.CurrentPassword) || string.IsNullOrWhiteSpace(model.NewPassword))
                return BadRequest(new { success = false, message = "Thiếu dữ liệu." });

            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == me.UserId);
            if (user == null) return NotFound(new { success = false, message = "Không tìm thấy người dùng." });

            if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash))
                return Ok(new { success = false, message = "Mật khẩu hiện tại không đúng." });

            if (model.NewPassword.Length < 6)
                return Ok(new { success = false, message = "Mật khẩu mới tối thiểu 6 ký tự." });

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            await _db.SaveChangesAsync();

            return Ok(new { success = true, message = "Đổi mật khẩu thành công." });
        }
    }
}
