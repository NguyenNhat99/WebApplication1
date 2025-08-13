using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Helpers;
using WebApplication1.Models;

namespace WebApplication1.Areas.PrivateSite.Controllers
{
    [Area("PrivateSite")]
    [CustomAuthorize(Roles = "Admin")]

    public class ProductController : Controller
    {
        private readonly FastFoodContext _db;
        private readonly IWebHostEnvironment _env;
        public ProductController(FastFoodContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? name, int page = 1, int pageSize = 10)
        {
            var query = _db.Foods.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(name))
            {
                string kw = name.Trim();
                query = query.Where(f => f.Name.Contains(kw));
            }

            int total = await query.CountAsync();
            int noOfPages = (int)Math.Ceiling(total / (double)pageSize);
            if (noOfPages == 0) noOfPages = 1;
            if (page < 1) page = 1;
            if (page > noOfPages) page = noOfPages;

            var foods = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.NoOfPages = noOfPages;
            ViewBag.DisplayPage = Math.Max(0, page - 3);
            ViewBag.Search = name ?? "";

            return View(foods);
        }

        [HttpGet]
        public async Task<IActionResult> Insert()
        {
            await LoadCategoriesAsync();
            return View(new Food { IsActive = true, ImageUrl = "/assets/img/no-image.png" });
        }

        private async Task<(List<string> errors, Dictionary<string, List<string>> fieldErrors)>
            ValidateFoodAsync(Food model, IFormFile? avatar)
        {
            var errors = new List<string>();
            var fieldErrors = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            void AddFieldError(string field, string message)
            {
                if (!fieldErrors.TryGetValue(field, out var list))
                {
                    list = new List<string>();
                    fieldErrors[field] = list;
                }
                list.Add(message);
            }

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                errors.Add("Vui lòng nhập tên sản phẩm.");
                AddFieldError(nameof(model.Name), "Tên sản phẩm không được để trống.");
            }
            else if (model.Name.Length > 200)
            {
                errors.Add("Tên sản phẩm tối đa 200 ký tự.");
                AddFieldError(nameof(model.Name), "Tối đa 200 ký tự.");
            }

            if (model.Price <= 0)
            {
                AddFieldError(nameof(model.Price), "Giá phải lớn hơn hoặc bằng 0.");
            }

            if (model.CategoryId.HasValue)
            {
                var catOk = await _db.Categories.AnyAsync(c => c.Id == model.CategoryId.Value);
                if (!catOk)
                {
                    errors.Add("Danh mục không hợp lệ.");
                    AddFieldError(nameof(model.CategoryId), "Danh mục không hợp lệ.");
                }
            }

            if (avatar is { Length: > 0 })
            {
                var ext = Path.GetExtension(avatar.FileName).ToLowerInvariant();
                var okExt = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                if (!okExt.Contains(ext))
                {
                    errors.Add("Ảnh phải là JPG/PNG/WebP.");
                    AddFieldError("avatar", "Ảnh phải là JPG/PNG/WebP.");
                }
                if (avatar.Length > 2 * 1024 * 1024)
                {
                    errors.Add("Ảnh tối đa 2MB.");
                    AddFieldError("avatar", "Kích thước ảnh tối đa 2MB.");
                }
            }

            return (errors, fieldErrors);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Insert(Food model, IFormFile? avatar)
        {
            // 1) Validate thủ công
            var (errors, fieldErrors) = await ValidateFoodAsync(model, avatar);
            if (errors.Any())
            {
                await LoadCategoriesAsync();
                ViewBag.Errors = errors;
                ViewBag.FieldErrors = fieldErrors;
                return View(model);
            }

            // 2) Upload ảnh (nếu có)
            if (avatar is { Length: > 0 })
            {
                var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads", "foods");
                if (!Directory.Exists(uploadsRoot)) Directory.CreateDirectory(uploadsRoot);

                var fileName = $"{Guid.NewGuid():N}{Path.GetExtension(avatar.FileName)}";
                var fullPath = Path.Combine(uploadsRoot, fileName);
                using var fs = new FileStream(fullPath, FileMode.Create);
                await avatar.CopyToAsync(fs);

                model.ImageUrl = $"/uploads/foods/{fileName}";
            }

            model.ImageUrl ??= "/assets/img/no-image.png";

            _db.Foods.Add(model);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

       


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var food = await _db.Foods.FindAsync(id);
            if (food == null) return NotFound();

            food.IsActive = !food.IsActive;
            await _db.SaveChangesAsync();
            return Ok(new { ok = true, status = food.IsActive });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var food = await _db.Foods.FindAsync(id);
            if (food == null) return NotFound();

            _db.Foods.Remove(food);
            await _db.SaveChangesAsync();
            return Ok(new { ok = true });
        }
        // GET: /privatesite/product/edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var food = await _db.Foods.FindAsync(id);
            if (food == null) return NotFound();

            await LoadCategoriesAsync();
            return View(food);
        }

        // POST: /privatesite/product/edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [FromForm] Food model,
            IFormFile? avatar,
            bool removeImage = false)
        {
            var food = await _db.Foods.FindAsync(id);
            if (food == null) return NotFound();

            if (string.IsNullOrWhiteSpace(model.Name))
                ModelState.AddModelError(nameof(model.Name), "Vui lòng nhập tên sản phẩm.");

            if (model.Price < 0)
                ModelState.AddModelError(nameof(model.Price), "Giá phải >= 0.");

            if (model.CategoryId.HasValue)
            {
                var catOk = await _db.Categories.AnyAsync(c => c.Id == model.CategoryId.Value);
                if (!catOk)
                    ModelState.AddModelError(nameof(model.CategoryId), "Danh mục không hợp lệ.");
            }
        
            food.Name = model.Name;
            food.Description = model.Description;
            food.Price = model.Price;
            food.CategoryId = model.CategoryId;
            food.IsActive = model.IsActive;

            var defaultImage = "/assets/img/no-image.png";

            if (removeImage)
            {
                if (!string.IsNullOrEmpty(food.ImageUrl) && food.ImageUrl.StartsWith("/uploads/foods/"))
                {
                    var oldPath = Path.Combine(_env.WebRootPath, food.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }
                food.ImageUrl = defaultImage;
            }

            if (avatar is { Length: > 0 })
            {
                if (!string.IsNullOrEmpty(food.ImageUrl) && food.ImageUrl.StartsWith("/uploads/foods/"))
                {
                    var oldPath = Path.Combine(_env.WebRootPath, food.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads", "foods");
                if (!Directory.Exists(uploadsRoot)) Directory.CreateDirectory(uploadsRoot);

                var fileName = $"{Guid.NewGuid():N}{Path.GetExtension(avatar.FileName)}";
                var fullPath = Path.Combine(uploadsRoot, fileName);
                using var fs = new FileStream(fullPath, FileMode.Create);
                await avatar.CopyToAsync(fs);

                food.ImageUrl = $"/uploads/foods/{fileName}";
            }

            food.ImageUrl ??= defaultImage;

            await _db.SaveChangesAsync();
            TempData["Toast"] = "Đã cập nhật sản phẩm!";
            await LoadCategoriesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadCategoriesAsync()
        {
            var cats = await _db.Categories
                                .AsNoTracking()
                                .OrderBy(c => c.CategoryName)
                                .ToListAsync();
            ViewData["Categories"] = cats;
        }
    }
}
