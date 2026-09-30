using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalExpenseTracker.Data;
using PersonalExpenseTracker.Models;

namespace PersonalExpenseTracker.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            string userId =
                _userManager.GetUserId(User)!;

            var orders = await _db.Orders
                .Where(o => o.ApplicationUserId == userId)
                .Include(o => o.OrderItems)
                .Include(o => o.Shipment)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            string userId =
                _userManager.GetUserId(User)!;

            Order? order = await _db.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Shipment)
                .FirstOrDefaultAsync(
                    o => o.Id == id &&
                         o.ApplicationUserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFromCart()
        {
            string? json =
                HttpContext.Session.GetString("ShoppingCart");

            List<CartItem> cart =
                string.IsNullOrWhiteSpace(json)
                    ? new List<CartItem>()
                    : JsonSerializer.Deserialize<List<CartItem>>(json)
                        ?? new List<CartItem>();

            if (cart.Count == 0)
            {
                TempData["error"] =
                    "Your cart is empty.";

                return RedirectToAction(
                    "Index",
                    "Cart");
            }

            string userId =
                _userManager.GetUserId(User)!;

            Order order = new()
            {
                ApplicationUserId = userId,
                OrderDate = DateTime.Now,
                Status = "Pending",
                TotalAmount = cart.Sum(x => x.Total)
            };

            foreach (CartItem cartItem in cart)
            {
                order.OrderItems.Add(new OrderItem
                {
                    Description = cartItem.Description,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.UnitPrice
                });
            }

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            HttpContext.Session.Remove("ShoppingCart");

            TempData["success"] =
                "Order created successfully.";

            return RedirectToAction(
                nameof(Details),
                new { id = order.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(
            int id,
            string status)
        {
            string userId =
                _userManager.GetUserId(User)!;

            Order? order = await _db.Orders
                .FirstOrDefaultAsync(
                    o => o.Id == id &&
                         o.ApplicationUserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            string[] allowedStatuses =
            {
                "Pending",
                "Processing",
                "Paid",
                "Shipped",
                "Delivered"
            };

            if (!allowedStatuses.Contains(status))
            {
                return BadRequest();
            }

            order.Status = status;
            await _db.SaveChangesAsync();

            return RedirectToAction(
                nameof(Details),
                new { id });
        }
    }
}