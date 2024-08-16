using Microsoft.AspNetCore.Mvc;
using SmarthStore.Models;
using SmarthStore.Services;

namespace SmartStore.Controllers
{
    public class StoreController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly int pageSize = 8;
        public StoreController(ApplicationDbContext context) 
        { 
            _context = context;
        }
        public IActionResult Index(int pageIndex)
        {
            IQueryable<Product> query = _context.Products;

            query = query.OrderByDescending(p => p.Id);

            if(pageIndex < 1) 
            {
                pageIndex = 1;
            }

            decimal count = query.Count();
            int totalPages = (int)Math.Ceiling(count / pageSize);
            query = query.Skip((pageIndex - 1) * pageIndex).Take(pageSize);   

            var products = query.ToList();

            ViewBag.Products = products;
            ViewBag.PageIndex= pageIndex;
            ViewBag.TotalPages = totalPages;

            return View();
        }
    }
}
