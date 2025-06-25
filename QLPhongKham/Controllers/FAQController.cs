using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLPhongKham.Data;
using QLPhongKham.Models;

namespace QLPhongKham.Controllers
{
    public class FAQController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FAQController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: FAQ
        public async Task<IActionResult> Index(string? category)
        {
            var query = _context.FAQs.Where(f => f.IsActive);

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(f => f.Category == category);
            }

            var faqs = await query
                .OrderBy(f => f.Category)
                .ThenBy(f => f.DisplayOrder)
                .ThenBy(f => f.Question)
                .ToListAsync();

            // Nhóm FAQs theo category
            var groupedFAQs = faqs.GroupBy(f => f.Category).ToList();

            // Lấy danh sách categories
            var categories = await _context.FAQs
                .Where(f => f.IsActive)
                .Select(f => f.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.CurrentCategory = category;
            ViewBag.GroupedFAQs = groupedFAQs;

            return View(faqs);
        }

        // GET: FAQ/Search
        public async Task<IActionResult> Search(string? keyword)
        {
            var query = _context.FAQs.Where(f => f.IsActive);

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(f => f.Question.Contains(keyword) || f.Answer.Contains(keyword));
            }

            var faqs = await query
                .OrderBy(f => f.Category)
                .ThenBy(f => f.DisplayOrder)
                .ToListAsync();

            ViewBag.Keyword = keyword;
            ViewBag.SearchResults = faqs;

            return View("Index", faqs);
        }

        // POST: FAQ/IncrementView
        [HttpPost]
        public async Task<IActionResult> IncrementView(int id)
        {
            var faq = await _context.FAQs.FindAsync(id);
            if (faq != null)
            {
                faq.ViewCount++;
                await _context.SaveChangesAsync();
            }
            return Json(new { success = true });
        }
    }
}
