using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using WebApplication1.Helpers; // SessionExtensions
using WebApplication1.Models;
using WebApplication1.ViewModel;

namespace WebApplication1.Areas.PrivateSite.Controllers
{
    [Area("PrivateSite")]
    [CustomAuthorize(Roles = "Admin")]
    public class ProfileAdminController : Controller
    {
        private readonly FastFoodContext _db;
        private const string CURRENT_USER_SESSION_KEY = "CURRENT_USER";

        public ProfileAdminController(FastFoodContext db)
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
                return RedirectToAction("Login", "Auth", new { area = "", returnUrl = Url.Action("Index", "ProfileAdmin", new { area = "PrivateSite" }) });

            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == me.UserId);
            if (user == null) return NotFound();

            var vm = new AdminProfileVM
            {
                UserId = user.UserId,
                Username = user.UserName,
                FullName = user.FullName ?? "",
                Email = user.Email ?? "",
                Phone = user.Phone,
                GroupName = user.Role ?? "Admin",
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(AdminProfileVM model)
        {
            var me = CurrentUser();
            if (me == null)
                return RedirectToAction("Login", "Auth", new { area = "", returnUrl = Url.Action("Index", "ProfileAdmin", new { area = "PrivateSite" }) });

            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == me.UserId);
            if (user == null) return NotFound();

            if (string.IsNullOrWhiteSpace(model.FullName))
            {
                ModelState.AddModelError(nameof(model.FullName), "Họ và tên không được để trống.");
            }

            model.UserId = user.UserId;
            model.Username = user.UserName;               
            model.Email = user.Email ?? "";            
            model.GroupName = user.Role ?? "Admin";        

            user.FullName = model.FullName.Trim();
            user.Phone = model.Phone?.Trim();
            await _db.SaveChangesAsync();

            me.FullName = user.FullName;
            HttpContext.Session.Set(CURRENT_USER_SESSION_KEY, me); 

            TempData["Toast"] = "Đã lưu thông tin tài khoản.";
            return RedirectToAction(nameof(Index));
        }

        // Đổi mật khẩu (JSON)
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
