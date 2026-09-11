using Microsoft.AspNetCore.Mvc;
using PersonalExpenseTracker.Models;
using PersonalExpenseTracker.Repository.IRepository;

namespace PersonalExpenseTracker.Controllers
{
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Category> categories = _unitOfWork.Category.GetAll();
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
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Add(category);
                _unitOfWork.Save();

                TempData["success"] = "Category created successfully.";

                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        public IActionResult Edit(int id)
        {
            Category? category = _unitOfWork.Category.Get(x => x.Id == id);

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
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Update(category);
                _unitOfWork.Save();

                TempData["success"] = "Category updated successfully.";

                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        public IActionResult Delete(int id)
        {
            Category? category = _unitOfWork.Category.Get(x => x.Id == id);

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
            Category? category = _unitOfWork.Category.Get(x => x.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            _unitOfWork.Category.Remove(category);
            _unitOfWork.Save();

            TempData["success"] = "Category deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Details(int id)
        {
            Category? category = _unitOfWork.Category.Get(x => x.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }
    }
}