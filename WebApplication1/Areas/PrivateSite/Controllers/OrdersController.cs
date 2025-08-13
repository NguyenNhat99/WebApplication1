using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Helpers;
using WebApplication1.Models;

namespace WebApplication1.Areas.PrivateSite.Controllers
{
    [Area("PrivateSite")]
    [CustomAuthorize(Roles = "Admin")]
    public class OrdersController : Controller
    {
        private readonly FastFoodContext _db;
        public OrdersController(FastFoodContext db) { _db = db; }

        // GET: /PrivateSite/Orders
        [HttpGet]
        public async Task<IActionResult> Index(string? status, string? q, int page = 1, int pageSize = 12)
        {
            // Query cơ bản
            var query = _db.Orders
                           .AsNoTracking()
                           .Include(o => o.User)
                           .OrderByDescending(o => o.OrderDate)
                           .AsQueryable();

            // Lọc trạng thái
            if (!string.IsNullOrWhiteSpace(status) && !status.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(o => o.Status == status);
            }

            // Tìm kiếm: mã đơn, user, địa chỉ, ghi chú
            if (!string.IsNullOrWhiteSpace(q))
            {
                var kw = q.Trim();
                query = query.Where(o =>
                    o.OrderId.ToString().Contains(kw) ||
                    (o.User != null && (o.User.FullName!.Contains(kw) || o.User.UserName!.Contains(kw))) ||
                    (o.Address != null && o.Address.Contains(kw)) ||
                    (o.Note != null && o.Note.Contains(kw))
                );
            }

            // Phân trang
            var total = await query.CountAsync();
            if (page < 1) page = 1;
            var noOfPages = (int)Math.Ceiling(total / (double)pageSize);
            if (noOfPages == 0) noOfPages = 1;
            if (page > noOfPages) page = noOfPages;

            var orders = await query.Skip((page - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync();

            ViewBag.Page = page;
            ViewBag.NoOfPages = noOfPages;
            ViewBag.DisplayPage = Math.Max(1, page - 3);
            ViewBag.Search = q ?? "";
            ViewBag.Status = status ?? "all";
            return View(orders);
        }

        // GET: /PrivateSite/Orders/Detail/5
        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var order = await _db.Orders
                .AsNoTracking()
                .Include(o => o.User)
                .Include(o => o.OrderDetails!)!.ThenInclude(d => d.Food)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null) return NotFound();
            return View(order);
        }

        // POST: /PrivateSite/Orders/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var allow = new[] { "Pending", "Completed", "Cancelled" };
            if (!allow.Contains(status)) return BadRequest(new { ok = false, message = "Trạng thái không hợp lệ." });

            var order = await _db.Orders.FirstOrDefaultAsync(o => o.OrderId == id);
            if (order == null) return NotFound(new { ok = false });

            order.Status = status;
            await _db.SaveChangesAsync();
            return Json(new { ok = true, status });
        }

        // POST: /PrivateSite/Orders/Delete
        // Lưu ý: Chỉ nên cho xoá các đơn Cancelled (hoặc để quản trị tự do tuỳ chính sách)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _db.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.OrderId == id);
            if (order == null) return NotFound(new { ok = false });

            // chính sách demo: chỉ cho xoá nếu Cancelled
            if (!string.Equals(order.Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { ok = false, message = "Chỉ xoá đơn đã huỷ." });

            _db.OrderDetails.RemoveRange(order.OrderDetails ?? []);
            _db.Orders.Remove(order);
            await _db.SaveChangesAsync();
            return Json(new { ok = true });
        }
    }
}
