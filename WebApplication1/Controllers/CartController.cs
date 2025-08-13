using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Helpers; // SessionExtensions
using WebApplication1.Models;
using WebApplication1.ViewModel;

namespace WebApplication1.Controllers
{
    public class CartController : Controller
    {
        private readonly FastFoodContext _context;
        private const string CART_KEY = "CART";
        private const string CURRENT_USER_SESSION_KEY = "CURRENT_USER";

        public CartController(FastFoodContext context)
        {
            _context = context;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Roles = "Customer")]
        public async Task<IActionResult> Checkout(string address, string? note, string? coupon)
        {
            address = (address ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(address))
            {
                TempData["Error"] = "Vui lòng nhập địa chỉ nhận hàng.";
                return RedirectToAction(nameof(Index));
            }
            if (address.Length > 250) 
            {
                TempData["Error"] = "Địa chỉ quá dài (tối đa 250 ký tự).";
                return RedirectToAction(nameof(Index));
            }

            var cart = GetCart();
            if (cart == null || !cart.Any())
            {
                TempData["Error"] = "Giỏ hàng trống.";
                return RedirectToAction(nameof(Index));
            }

            var me = HttpContext.Session.Get<UserSessionVm>(CURRENT_USER_SESSION_KEY);
            //if (me == null)
            //{
            //    var returnUrl = Url.Action(nameof(Index), "Cart");
            //    return RedirectToAction("Login", "Auth", new { returnUrl });
            //}

            var total = cart.Sum(x => x.Total); 
            if (!string.IsNullOrWhiteSpace(coupon) && coupon.Trim().ToUpper() == "GIAM10")
            {
                var giam = Math.Min(Math.Round(total * 0.10m, 0), 50000m);
                total = Math.Max(0, total - giam);
            }

            var order = new Order
            {
                UserId = me.UserId,
                OrderDate = DateTime.Now,
                TotalPrice = (int)total,
                Status = "Pending",
                Address = address,
                Note = note
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var details = cart.Select(i => new OrderDetail
            {
                OrderId = order.OrderId,
                FoodId = i.FoodId,
                Quantity = i.Quantity,
                Price = (int)i.Price   
            }).ToList();

            _context.OrderDetails.AddRange(details);
            await _context.SaveChangesAsync();

            SaveCart(new List<CartItemVM>());
            TempData["Toast"] = "Đặt hàng thành công!";
            return RedirectToAction(nameof(Success), new { id = order.OrderId });
        }


        [HttpGet]
        public async Task<IActionResult> Success(int id)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.OrderDetails!)
                    .ThenInclude(d => d.Food)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null) return NotFound();

            return View(order); 
        }
        private List<CartItemVM> GetCart()
        {
            return HttpContext.Session.Get<List<CartItemVM>>(CART_KEY) ?? new List<CartItemVM>();
        }

        private void SaveCart(List<CartItemVM> cart)
        {
            HttpContext.Session.Set(CART_KEY, cart);
        }

        public IActionResult Index()
        {
            var cart = GetCart();
            ViewBag.Total = cart.Sum(x => x.Total);
            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(int id, int quantity = 1)
        {
            if (quantity < 1) quantity = 1;

            var food = _context.Foods.FirstOrDefault(f => f.FoodId == id && f.IsActive);
            if (food == null)
            {
                // AJAX -> JSON; non-AJAX -> 404
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return NotFound(new { ok = false, message = "Món không tồn tại hoặc ngừng bán." });
                return NotFound();
            }

            var cart = GetCart();
            var existingItem = cart.FirstOrDefault(x => x.FoodId == id);

            if (existingItem != null) existingItem.Quantity += quantity;
            else
            {
                cart.Add(new CartItemVM
                {
                    FoodId = food.FoodId,
                    Name = food.Name,
                    ImageUrl = string.IsNullOrWhiteSpace(food.ImageUrl) ? "/assets/img/no-image.png" : food.ImageUrl,
                    Price = food.Price,
                    Quantity = quantity
                });
            }

            SaveCart(cart);
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { ok = true, count = cart.Sum(x => x.Quantity) });

            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult Update(int id, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.FoodId == id);
            if (item != null && quantity > 0)
            {
                item.Quantity = quantity;
                SaveCart(cart);
            }
            return RedirectToAction("Index");
        }

        public IActionResult Remove(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.FoodId == id);
            if (item != null)
            {
                cart.Remove(item);
                SaveCart(cart);
            }
            return RedirectToAction("Index");
        }

        public IActionResult Clear()
        {
            SaveCart(new List<CartItemVM>());
            return RedirectToAction("Index");
        }
    }
}
