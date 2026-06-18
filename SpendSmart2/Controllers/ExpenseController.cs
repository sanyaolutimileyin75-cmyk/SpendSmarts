using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpendSmart2.Data;
using SpendSmart2.Models;
using System.Security.Claims;

namespace SpendSmart2.Controllers
{
    [Authorize]
    public class ExpenseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExpenseController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Expense
        public IActionResult Index(string? search, string? category, string? sort)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var expenses = _context.Expenses
                .Where(e => e.UserId == userId)
                .AsQueryable();

            // ===== SEARCH =====
            if (!string.IsNullOrEmpty(search))
            {
                expenses = expenses.Where(e =>
                    e.Name!.Contains(search) ||
                    e.Category!.Contains(search));
            }

            // ===== FILTER BY CATEGORY =====
            if (!string.IsNullOrEmpty(category))
            {
                expenses = expenses.Where(e => e.Category == category);
            }

            // ===== SORT =====
            expenses = sort switch
            {
                "amount_asc" => expenses.OrderBy(e => e.Amount),
                "amount_desc" => expenses.OrderByDescending(e => e.Amount),
                "date_asc" => expenses.OrderBy(e => e.Date),
                _ => expenses.OrderByDescending(e => e.Date)
            };

            // Pass filter values back to view
            ViewBag.Search = search;
            ViewBag.Category = category;
            ViewBag.Sort = sort;
            ViewBag.TotalFiltered = expenses.Sum(e => e.Amount);

            return View(expenses.ToList());
        }

        // GET: /Expense/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Expense/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Expense model)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            model.UserId = userId;
            model.Date = DateTime.Now;
            ModelState.Remove("User");

            if (!ModelState.IsValid)
                return View(model);

            _context.Expenses.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // GET: /Expense/Edit/5
        public IActionResult Edit(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var expense = _context.Expenses
                .FirstOrDefault(e => e.Id == id && e.UserId == userId);

            if (expense == null)
                return NotFound();

            return View(expense);
        }

        // POST: /Expense/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Expense model)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            model.UserId = userId;
            ModelState.Remove("User");

            if (!ModelState.IsValid)
                return View(model);

            var expense = _context.Expenses
                .FirstOrDefault(e => e.Id == id && e.UserId == userId);

            if (expense == null)
                return NotFound();

            expense.Name = model.Name;
            expense.Category = model.Category;
            expense.Amount = model.Amount;
            expense.Date = model.Date;

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // POST: /Expense/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var expense = _context.Expenses
                .FirstOrDefault(e => e.Id == id && e.UserId == userId);

            if (expense == null)
                return NotFound();

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
