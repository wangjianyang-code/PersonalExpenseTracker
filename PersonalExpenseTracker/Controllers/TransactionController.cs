using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PersonalExpenseTracker.Models;
using PersonalExpenseTracker.Repository.IRepository;

namespace PersonalExpenseTracker.Controllers
{
    public class TransactionController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public TransactionController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Transaction> transactions =
                _unitOfWork.Transaction.GetAll("Category");

            return View(transactions);
        }

        public IActionResult Create()
        {
            ViewBag.CategoryList = _unitOfWork.Category
                .GetAll()
                .Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                });

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Transaction.Add(transaction);
                _unitOfWork.Save();

                TempData["success"] = "Transaction created successfully.";

                return RedirectToAction(nameof(Index));
            }

            ViewBag.CategoryList = _unitOfWork.Category
                .GetAll()
                .Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                });

            return View(transaction);
        }

        public IActionResult Edit(int id)
        {
            Transaction? transaction =
                _unitOfWork.Transaction.Get(x => x.Id == id, "Category");

            if (transaction == null)
            {
                return NotFound();
            }

            ViewBag.CategoryList = _unitOfWork.Category
                .GetAll()
                .Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                });

            return View(transaction);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Transaction.Update(transaction);
                _unitOfWork.Save();

                TempData["success"] = "Transaction updated successfully.";

                return RedirectToAction(nameof(Index));
            }

            ViewBag.CategoryList = _unitOfWork.Category
                .GetAll()
                .Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                });

            return View(transaction);
        }

        public IActionResult Delete(int id)
        {
            Transaction? transaction =
                _unitOfWork.Transaction.Get(x => x.Id == id, "Category");

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
            Transaction? transaction =
                _unitOfWork.Transaction.Get(x => x.Id == id);

            if (transaction == null)
            {
                return NotFound();
            }

            _unitOfWork.Transaction.Remove(transaction);
            _unitOfWork.Save();

            TempData["success"] = "Transaction deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Details(int id)
        {
            Transaction? transaction =
                _unitOfWork.Transaction.Get(x => x.Id == id, "Category");

            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }
    }
}