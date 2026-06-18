using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpendSmart2.Data;
using System.Security.Claims;
using System.Text.Json;

namespace SpendSmart2.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var expenses = _context.Expenses
                .Where(e => e.UserId == userId)
                .ToList();

            // ===== STATS =====
            ViewBag.TotalExpenses = expenses.Sum(e => e.Amount);
            ViewBag.ExpenseCount = expenses.Count;
            ViewBag.UserName = User.FindFirstValue(ClaimTypes.Name);

            // ===== RECENT EXPENSES =====
            ViewBag.RecentExpenses = expenses
                .OrderByDescending(e => e.Date)
                .Take(5)
                .ToList();

            // ===== HIGHEST EXPENSE =====
            ViewBag.HighestExpense = expenses.Any()
                ? expenses.Max(e => e.Amount) : 0;

            // ===== THIS MONTH TOTAL =====
            var thisMonth = expenses
                .Where(e => e.Date.Month == DateTime.Now.Month
                && e.Date.Year == DateTime.Now.Year)
                .Sum(e => e.Amount);
            ViewBag.ThisMonthTotal = thisMonth;

            // ===== CATEGORY CHART DATA =====
            var categoryData = expenses
                .GroupBy(e => e.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    Total = g.Sum(e => e.Amount)
                })
                .OrderByDescending(x => x.Total)
                .ToList();

            ViewBag.CategoryLabels = JsonSerializer.Serialize(
                categoryData.Select(c => c.Category).ToList());
            ViewBag.CategoryTotals = JsonSerializer.Serialize(
                categoryData.Select(c => c.Total).ToList());

            // ===== MONTHLY CHART DATA (Last 6 months) =====
            var monthlyData = new List<object>();
            var monthLabels = new List<string>();

            for (int i = 5; i >= 0; i--)
            {
                var date = DateTime.Now.AddMonths(-i);
                var total = expenses
                    .Where(e => e.Date.Month == date.Month
                    && e.Date.Year == date.Year)
                    .Sum(e => e.Amount);

                monthLabels.Add(date.ToString("MMM yyyy"));
                monthlyData.Add(total);
            }

            ViewBag.MonthLabels = JsonSerializer.Serialize(monthLabels);
            ViewBag.MonthlyTotals = JsonSerializer.Serialize(monthlyData);

            return View();
        }
    }
}
