using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLPhongKham.Data;
using QLPhongKham.Models;

namespace QLPhongKham.Controllers
{
    public class ArticleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ArticleController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Article
        public async Task<IActionResult> Index(string? category, int page = 1, int pageSize = 6)
        {
            var query = _context.Articles.Where(a => a.IsActive);

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(a => a.Category == category);
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var articles = await query
                .OrderByDescending(a => a.IsFeatured)
                .ThenByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Lấy danh sách categories để filter
            var categories = await _context.Articles
                .Where(a => a.IsActive)
                .Select(a => a.Category)
                .Distinct()
                .ToListAsync();

            // Lấy bài viết nổi bật
            var featuredArticles = await _context.Articles
                .Where(a => a.IsActive && a.IsFeatured)
                .OrderByDescending(a => a.CreatedAt)
                .Take(3)
                .ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.CurrentCategory = category;
            ViewBag.FeaturedArticles = featuredArticles;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;

            return View(articles);
        }

        // GET: Article/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var article = await _context.Articles
                .FirstOrDefaultAsync(a => a.ArticleId == id && a.IsActive);

            if (article == null)
            {
                return NotFound();
            }

            // Tăng lượt xem
            article.ViewCount++;
            await _context.SaveChangesAsync();

            // Lấy bài viết liên quan (cùng category)
            var relatedArticles = await _context.Articles
                .Where(a => a.IsActive && a.Category == article.Category && a.ArticleId != id)
                .OrderByDescending(a => a.CreatedAt)
                .Take(4)
                .ToListAsync();

            ViewBag.RelatedArticles = relatedArticles;

            return View(article);
        }

        // GET: Article/Search
        public async Task<IActionResult> Search(string? keyword, int page = 1, int pageSize = 6)
        {
            var query = _context.Articles.Where(a => a.IsActive);

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(a => a.Title.Contains(keyword) || 
                                        a.Summary.Contains(keyword) || 
                                        a.Content.Contains(keyword) ||
                                        (a.Tags != null && a.Tags.Contains(keyword)));
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var articles = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Keyword = keyword;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;

            return View("Index", articles);
        }
    }
}
