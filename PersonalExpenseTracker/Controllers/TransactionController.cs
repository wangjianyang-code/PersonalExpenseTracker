using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PersonalExpenseTracker.Models;
using PersonalExpenseTracker.Repository.IRepository;

namespace PersonalExpenseTracker.Controllers
{
    [Authorize]
    public class TransactionController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public TransactionController(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        private string GetCurrentUserId()
        {
            return _userManager.GetUserId(User)!;
        }

        private void LoadCategoryList()
        {
            string userId = GetCurrentUserId();

            ViewBag.CategoryList = _unitOfWork.Category
                .GetAll()
                .Where(c => c.ApplicationUserId == userId)
                .Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                });
        }

        public IActionResult Index()
        {
            string userId = GetCurrentUserId();

            IEnumerable<Transaction> transactions =
                _unitOfWork.Transaction
                    .GetAll("Category")
                    .Where(t => t.ApplicationUserId == userId);

            return View(transactions);
        }

        public IActionResult Create()
        {
            LoadCategoryList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Transaction transaction)
        {
            string userId = GetCurrentUserId();

            Category? category = _unitOfWork.Category.Get(
                c => c.Id == transaction.CategoryId &&
                     c.ApplicationUserId == userId);

            if (category == null)
            {
                ModelState.AddModelError(
                    nameof(Transaction.CategoryId),
                    "Please select a valid category.");
            }

            transaction.ApplicationUserId = userId;
            ModelState.Remove(nameof(Transaction.ApplicationUserId));

            if (ModelState.IsValid)
            {
                _unitOfWork.Transaction.Add(transaction);
                _unitOfWork.Save();

                TempData["success"] =
                    "Transaction created successfully.";

                return RedirectToAction(nameof(Index));
            }

            LoadCategoryList();
            return View(transaction);
        }

        public IActionResult Edit(int id)
        {
            string userId = GetCurrentUserId();

            Transaction? transaction =
                _unitOfWork.Transaction.Get(
                    t => t.Id == id &&
                         t.ApplicationUserId == userId,
                    "Category");

            if (transaction == null)
            {
                return NotFound();
            }

            LoadCategoryList();
            return View(transaction);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Transaction transaction)
        {
            string userId = GetCurrentUserId();

            Transaction? transactionFromDb =
                _unitOfWork.Transaction.Get(
                    t => t.Id == transaction.Id &&
                         t.ApplicationUserId == userId);

            if (transactionFromDb == null)
            {
                return NotFound();
            }

            Category? category =
                _unitOfWork.Category.Get(
                    c => c.Id == transaction.CategoryId &&
                         c.ApplicationUserId == userId);

            if (category == null)
            {
                ModelState.AddModelError(
                    nameof(Transaction.CategoryId),
                    "Please select a valid category.");
            }

            ModelState.Remove(nameof(Transaction.ApplicationUserId));

            if (ModelState.IsValid)
            {
                transactionFromDb.Description = transaction.Description;
                transactionFromDb.Type = transaction.Type;
                transactionFromDb.Amount = transaction.Amount;
                transactionFromDb.Date = transaction.Date;
                transactionFromDb.CategoryId = transaction.CategoryId;
                transactionFromDb.Notes = transaction.Notes;

                _unitOfWork.Transaction.Update(transactionFromDb);
                _unitOfWork.Save();

                TempData["success"] =
                    "Transaction updated successfully.";

                return RedirectToAction(nameof(Index));
            }

            transaction.ApplicationUserId = userId;
            LoadCategoryList();

            return View(transaction);
        }

        public IActionResult Details(int id)
        {
            string userId = GetCurrentUserId();

            Transaction? transaction =
                _unitOfWork.Transaction.Get(
                    t => t.Id == id &&
                         t.ApplicationUserId == userId,
                    "Category");

            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        public IActionResult Delete(int id)
        {
            string userId = GetCurrentUserId();

            Transaction? transaction =
                _unitOfWork.Transaction.Get(
                    t => t.Id == id &&
                         t.ApplicationUserId == userId,
                    "Category");

            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int id)
        {
            string userId = GetCurrentUserId();

            Transaction? transaction =
                _unitOfWork.Transaction.Get(
                    t => t.Id == id &&
                         t.ApplicationUserId == userId);

            if (transaction == null)
            {
                return NotFound();
            }

            _unitOfWork.Transaction.Remove(transaction);
            _unitOfWork.Save();

            TempData["success"] =
                "Transaction deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}