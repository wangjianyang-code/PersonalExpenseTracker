using Microsoft.AspNetCore.Mvc;
using PersonalExpenseTracker.Repository.IRepository;

namespace PersonalExpenseTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var transactions = _unitOfWork.Transaction.GetAll();

            decimal totalIncome = transactions
                .Where(t => t.Type == "Income")
                .Sum(t => t.Amount);

            decimal totalExpense = transactions
                .Where(t => t.Type == "Expense")
                .Sum(t => t.Amount);

            decimal balance = totalIncome - totalExpense;

            ViewBag.TotalIncome = totalIncome;
            ViewBag.TotalExpense = totalExpense;
            ViewBag.Balance = balance;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}