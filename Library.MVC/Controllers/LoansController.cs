using Library.MVC.Data;
using Library.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Library.MVC.Controllers
{
    public class LoansController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoansController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var loans = await _context.Loans
                .Include(l => l.Book)
                .Include(l => l.Member)
                .ToListAsync();
            return View(loans);
        }

        public IActionResult Create()
        {
            ViewBag.Books = new SelectList(_context.Books.Where(b => b.IsAvailable), "Id", "Title");
            ViewBag.Members = new SelectList(_context.Members, "Id", "FullName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Loan loan)
        {
            // Check book is still available
            var book = await _context.Books.FindAsync(loan.BookId);
            if (book == null || !book.IsAvailable)
            {
                ModelState.AddModelError("", "This book is not available for loan.");
                ViewBag.Books = new SelectList(_context.Books.Where(b => b.IsAvailable), "Id", "Title");
                ViewBag.Members = new SelectList(_context.Members, "Id", "FullName");
                return View(loan);
            }

            loan.LoanDate = DateTime.Today;
            loan.DueDate = DateTime.Today.AddDays(14);
            book.IsAvailable = false;

            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkReturned(int id)
        {
            var loan = await _context.Loans.Include(l => l.Book).FirstOrDefaultAsync(l => l.Id == id);
            if (loan == null) return NotFound();

            loan.ReturnedDate = DateTime.Today;
            loan.Book.IsAvailable = true;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}