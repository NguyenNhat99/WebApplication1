using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly FastFoodContext _db;

        public HomeController(ILogger<HomeController> logger, FastFoodContext db)
        {
            _logger = logger;
            _db = db;
        }

        // /?categoryId=1&q=ram&page=1
        [HttpGet]
        public async Task<IActionResult> Index(int? categoryId, string? q, int page = 1, int pageSize = 12)
        {
            // Danh mục
            var categories = await _db.Categories
                                      .AsNoTracking()
                                      .OrderBy(c => c.CategoryName)
                                      .ToListAsync();
            ViewData["Categories"] = categories;
            ViewBag.CategoryId = categoryId;
            ViewBag.Query = q ?? "";

            // Foods đang bán
            var foodsQuery = _db.Foods.AsNoTracking().Where(f => f.IsActive);

            if (categoryId.HasValue)
                foodsQuery = foodsQuery.Where(f => f.CategoryId == categoryId.Value);

            if (!string.IsNullOrWhiteSpace(q))
            {
                var kw = q.Trim();
                foodsQuery = foodsQuery.Where(f => f.Name.Contains(kw));
            }

            // Phân trang
            var total = await foodsQuery.CountAsync();
            if (page < 1) page = 1;
            var noOfPages = (int)Math.Ceiling(total / (double)pageSize);
            if (noOfPages == 0) noOfPages = 1;
            if (page > noOfPages) page = noOfPages;

            var foods = await foodsQuery
                .OrderByDescending(f => f.FoodId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.NoOfPages = noOfPages;

            // Trả về view kèm danh sách foods
            return View(foods);
        }

        public IActionResult Privacy() => View();
    }
}
