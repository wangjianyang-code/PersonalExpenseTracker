using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalExpenseTracker.Data;
using PersonalExpenseTracker.Models;
using Stripe;
using Stripe.Checkout;

namespace PersonalExpenseTracker.Controllers
{
    [Authorize]
    [Route("api/payment")]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public PaymentController(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
        {
            _db = db;
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpPost("create-checkout-session/{orderId:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCheckoutSession(
            int orderId)
        {
            string userId =
                _userManager.GetUserId(User)!;

            Order? order = await _db.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(
                    o => o.Id == orderId &&
                         o.ApplicationUserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            if (order.OrderItems.Count == 0)
            {
                return BadRequest(
                    "The order does not contain any items.");
            }

            string? secretKey =
                _configuration["Stripe:SecretKey"];

            if (string.IsNullOrWhiteSpace(secretKey))
            {
                return StatusCode(
                    500,
                    "Stripe secret key is not configured.");
            }

            StripeConfiguration.ApiKey = secretKey;

            string baseUrl =
                $"{Request.Scheme}://{Request.Host}";

            var lineItems = order.OrderItems
                .Select(item =>
                    new SessionLineItemOptions
                    {
                        Quantity = item.Quantity,

                        PriceData =
                            new SessionLineItemPriceDataOptions
                            {
                                Currency = "nzd",

                                UnitAmount =
                                    (long)Math.Round(
                                        item.UnitPrice * 100m),

                                ProductData =
                                    new SessionLineItemPriceDataProductDataOptions
                                    {
                                        Name = item.Description
                                    }
                            }
                    })
                .ToList();

            var options =
                new SessionCreateOptions
                {
                    Mode = "payment",

                    LineItems = lineItems,

                    SuccessUrl =
                        $"{baseUrl}/api/payment/success" +
                        $"?orderId={order.Id}" +
                        "&session_id={CHECKOUT_SESSION_ID}",

                    CancelUrl =
                        $"{baseUrl}/Order/Details/{order.Id}",

                    Metadata =
                        new Dictionary<string, string>
                        {
                            {
                                "OrderId",
                                order.Id.ToString()
                            },

                            {
                                "UserId",
                                userId
                            }
                        }
                };

            var service = new SessionService();

            Session session =
                await service.CreateAsync(options);

            return Redirect(session.Url);
        }

        [HttpGet("success")]
        public async Task<IActionResult> Success(
            int orderId,
            string session_id)
        {
            string userId =
                _userManager.GetUserId(User)!;

            Order? order =
                await _db.Orders.FirstOrDefaultAsync(
                    o => o.Id == orderId &&
                         o.ApplicationUserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            string? secretKey =
                _configuration["Stripe:SecretKey"];

            if (string.IsNullOrWhiteSpace(secretKey))
            {
                return StatusCode(
                    500,
                    "Stripe secret key is not configured.");
            }

            StripeConfiguration.ApiKey = secretKey;

            var service = new SessionService();

            Session session =
                await service.GetAsync(session_id);

            if (session.PaymentStatus == "paid")
            {
                order.Status = "Paid";
                order.PaymentReference = session.Id;

                await _db.SaveChangesAsync();

                TempData["success"] =
                    "Payment completed successfully.";
            }
            else
            {
                TempData["error"] =
                    "Payment has not been completed.";
            }

            return RedirectToAction(
                "Details",
                "Order",
                new { id = order.Id });
        }

        [HttpGet("status/{orderId:int}")]
        public async Task<IActionResult> GetPaymentStatus(
            int orderId)
        {
            string userId =
                _userManager.GetUserId(User)!;

            Order? order =
                await _db.Orders.FirstOrDefaultAsync(
                    o => o.Id == orderId &&
                         o.ApplicationUserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                orderId = order.Id,
                status = order.Status,
                paymentReference = order.PaymentReference,
                totalAmount = order.TotalAmount
            });
        }
    }
}