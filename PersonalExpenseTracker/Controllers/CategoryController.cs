using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PersonalExpenseTracker.Models;
using PersonalExpenseTracker.Repository.IRepository;

namespace PersonalExpenseTracker.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public CategoryController(
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

        public IActionResult Index()
        {
            string userId = GetCurrentUserId();

            IEnumerable<Category> categories =
                _unitOfWork.Category
                    .GetAll()
                    .Where(c => c.ApplicationUserId == userId);

            return View(categories);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category category)
        {
            string userId = GetCurrentUserId();

            category.ApplicationUserId = userId;

            ModelState.Remove(nameof(Category.ApplicationUserId));

            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Add(category);
                _unitOfWork.Save();

                TempData["success"] =
                    "Category created successfully.";

                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        public IActionResult Edit(int id)
        {
            string userId = GetCurrentUserId();

            Category? category =
                _unitOfWork.Category.Get(
                    x => x.Id == id &&
                         x.ApplicationUserId == userId);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category category)
        {
            string userId = GetCurrentUserId();

            Category? categoryFromDb =
                _unitOfWork.Category.Get(
                    x => x.Id == category.Id &&
                         x.ApplicationUserId == userId);

            if (categoryFromDb == null)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(Category.ApplicationUserId));

            if (ModelState.IsValid)
            {
                categoryFromDb.Name = category.Name;
                categoryFromDb.Description = category.Description;

                _unitOfWork.Category.Update(categoryFromDb);
                _unitOfWork.Save();

                TempData["success"] =
                    "Category updated successfully.";

                return RedirectToAction(nameof(Index));
            }

            category.ApplicationUserId = userId;

            return View(category);
        }

        public IActionResult Delete(int id)
        {
            string userId = GetCurrentUserId();

            Category? category =
                _unitOfWork.Category.Get(
                    x => x.Id == id &&
                         x.ApplicationUserId == userId);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int id)
        {
            string userId = GetCurrentUserId();

            Category? category =
                _unitOfWork.Category.Get(
                    x => x.Id == id &&
                         x.ApplicationUserId == userId);

            if (category == null)
            {
                return NotFound();
            }

            bool categoryIsInUse =
                _unitOfWork.Transaction
                    .GetAll()
                    .Any(t =>
                        t.CategoryId == id &&
                        t.ApplicationUserId == userId);

            if (categoryIsInUse)
            {
                TempData["error"] =
                    "This category cannot be deleted because it is being used by one or more transactions.";

                return RedirectToAction(nameof(Index));
            }

            _unitOfWork.Category.Remove(category);
            _unitOfWork.Save();

            TempData["success"] =
                "Category deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Details(int id)
        {
            string userId = GetCurrentUserId();

            Category? category =
                _unitOfWork.Category.Get(
                    x => x.Id == id &&
                         x.ApplicationUserId == userId);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }
    }
}