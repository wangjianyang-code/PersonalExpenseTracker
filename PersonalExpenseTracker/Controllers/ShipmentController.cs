using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalExpenseTracker.Data;
using PersonalExpenseTracker.Models;

namespace PersonalExpenseTracker.Controllers
{
    [Authorize]
    public class ShipmentController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public ShipmentController(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Create(int orderId)
        {
            string userId =
                _userManager.GetUserId(User)!;

            Order? order = await _db.Orders
                .Include(o => o.Shipment)
                .FirstOrDefaultAsync(
                    o => o.Id == orderId &&
                         o.ApplicationUserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Shipment != null)
            {
                return RedirectToAction(
                    "Details",
                    "Order",
                    new { id = orderId });
            }

            return View(new Shipment
            {
                OrderId = orderId
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Shipment shipment)
        {
            string userId =
                _userManager.GetUserId(User)!;

            Order? order = await _db.Orders
                .Include(o => o.Shipment)
                .FirstOrDefaultAsync(
                    o => o.Id == shipment.OrderId &&
                         o.ApplicationUserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Shipment != null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "This order already has shipment information.");
            }

            if (!ModelState.IsValid)
            {
                return View(shipment);
            }

            shipment.Status = "Preparing";

            _db.Shipments.Add(shipment);

            order.Status = "Processing";

            await _db.SaveChangesAsync();

            return RedirectToAction(
                "Details",
                "Order",
                new { id = shipment.OrderId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkShipped(int id)
        {
            string userId =
                _userManager.GetUserId(User)!;

            Shipment? shipment = await _db.Shipments
                .Include(s => s.Order)
                .FirstOrDefaultAsync(
                    s => s.Id == id &&
                         s.Order != null &&
                         s.Order.ApplicationUserId == userId);

            if (shipment == null)
            {
                return NotFound();
            }

            shipment.Status = "Shipped";
            shipment.ShippedDate = DateTime.Now;

            shipment.Order!.Status = "Shipped";

            await _db.SaveChangesAsync();

            return RedirectToAction(
                "Details",
                "Order",
                new { id = shipment.OrderId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkDelivered(int id)
        {
            string userId =
                _userManager.GetUserId(User)!;

            Shipment? shipment = await _db.Shipments
                .Include(s => s.Order)
                .FirstOrDefaultAsync(
                    s => s.Id == id &&
                         s.Order != null &&
                         s.Order.ApplicationUserId == userId);

            if (shipment == null)
            {
                return NotFound();
            }

            shipment.Status = "Delivered";
            shipment.DeliveredDate = DateTime.Now;

            shipment.Order!.Status = "Delivered";

            await _db.SaveChangesAsync();

            return RedirectToAction(
                "Details",
                "Order",
                new { id = shipment.OrderId });
        }
    }
}