using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using WebApplication1.Helpers;

namespace WebApplication1.Areas.PrivateSite.Controllers
{
    [Area("PrivateSite")]
    [CustomAuthorize(Roles = "Admin")] 
    public class CategoriesController : Controller
    {
        private readonly FastFoodContext _db;
        private const int PageSize = 8;

        public CategoriesController(FastFoodContext db)
        {
            _db = db;
        }

        // GET: /privatesite/categories?page=1&name=abc
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, string? name = null)
        {
            var query = _db.Categories.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(x => x.CategoryName.Contains(name));

            var total = await query.CountAsync();
            var noOfPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
            page = Math.Clamp(page, 1, noOfPages);

            var items = await query
                .OrderBy(x => x.Id)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewData["Categories"] = items;
            ViewBag.Page = page;
            ViewBag.NoOfPages = noOfPages;
            ViewBag.DisplayPage = Math.Max(0, page - 3);
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult IndexPost(string? name)
            => RedirectToAction(nameof(Index), new { page = 1, name });

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Insert(int? Id, string CategoryName)
        {
            if (string.IsNullOrWhiteSpace(CategoryName))
            {
                TempData["updateItem"] = "Vui lòng nhập tên thể loại.";
                return RedirectToAction(nameof(Index));
            }

            // OPTIONAL: ràng buộc tên không trùng (case-insensitive)
            var existed = await _db.Categories
                .AnyAsync(x => x.CategoryName.ToLower() == CategoryName.ToLower()
                               && (!Id.HasValue || x.Id != Id.Value));
            if (existed)
            {
                TempData["updateItem"] = "Tên thể loại đã tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            if (Id.GetValueOrDefault() > 0)
            {
                var cat = await _db.Categories.FindAsync(Id.Value);
                if (cat == null)
                {
                    TempData["updateItem"] = "Không tìm thấy thể loại để cập nhật.";
                    return RedirectToAction(nameof(Index));
                }
                cat.CategoryName = CategoryName;
                await _db.SaveChangesAsync();
                TempData["updateItem"] = $"Đã cập nhật thể loại #{Id.Value}.";
            }
            else
            {
                var cat = new Category { CategoryName = CategoryName };
                _db.Categories.Add(cat);
                await _db.SaveChangesAsync();
                TempData["updateItem"] = $"Đã thêm thể loại mới (#{cat.Id}).";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("/api/categories/update/{id:int}")]
        public async Task<IActionResult> GetOne(int id)
        {
            var cat = await _db.Categories.AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == id);
            if (cat == null) return Json(new { success = false });

            return Json(new
            {
                success = true,
                data = new { categoryId = cat.Id, categoryName = cat.CategoryName }
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var cat = await _db.Categories.FindAsync(id);
            if (cat != null)
            {
                var foods = await _db.Foods.Where(f=>f.CategoryId == id).ToListAsync();
                _db.Foods.RemoveRange(foods);
                _db.Categories.Remove(cat);
                await _db.SaveChangesAsync();
                TempData["updateItem"] = $"Đã xóa thể loại #{id}.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
