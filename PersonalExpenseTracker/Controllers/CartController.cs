using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalExpenseTracker.Models;

namespace PersonalExpenseTracker.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private const string CartKey = "ShoppingCart";

        public IActionResult Index()
        {
            return View(GetCart());
        }

        public IActionResult Add()
        {
            return View(new CartItem());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(CartItem item)
        {
            if (!ModelState.IsValid)
            {
                return View(item);
            }

            List<CartItem> cart = GetCart();

            item.Id = cart.Count == 0
                ? 1
                : cart.Max(x => x.Id) + 1;

            cart.Add(item);
            SaveCart(cart);

            TempData["success"] =
                "Item added to cart successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int id)
        {
            List<CartItem> cart = GetCart();

            CartItem? item =
                cart.FirstOrDefault(x => x.Id == id);

            if (item != null)
            {
                cart.Remove(item);
                SaveCart(cart);
            }

            return RedirectToAction(nameof(Index));
        }

        private List<CartItem> GetCart()
        {
            string? json =
                HttpContext.Session.GetString(CartKey);

            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<CartItem>();
            }

            return JsonSerializer.Deserialize<List<CartItem>>(json)
                ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            string json =
                JsonSerializer.Serialize(cart);

            HttpContext.Session.SetString(
                CartKey,
                json);
        }
    }
}