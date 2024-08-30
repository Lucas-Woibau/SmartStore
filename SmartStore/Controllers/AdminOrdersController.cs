using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartStore.Models;
using SmartStore.Services;

namespace SmartStore.Controllers
{
    [Authorize(Roles = "admin")]
    [Route("/Admin/Orders/{action=Index}/{id?}")]
    public class AdminOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly int _pageSize = 5;

        public AdminOrdersController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index(int pageIndex)
        {
            IQueryable<Order> query = _context.Orders.Include(o => o.Client)
                .Include(o => o.Items).OrderByDescending(o => o.Id);

            if (pageIndex <= 0)
            {
                pageIndex = 1;
            }
            decimal count = query.Count();
            int totalPages = (int)Math.Ceiling(count / _pageSize);

            query = query.Skip((pageIndex - 1) * _pageSize).Take(_pageSize);

            var orders = query.ToList();

            ViewBag.Orders = orders;
            ViewBag.PageIndex = pageIndex;
            ViewBag.TotalPages = totalPages;

            return View();
        }

        public IActionResult Details(int id)
        {
            var order = _context.Orders.Include(_o => _o.Client).Include(o => o.Items)
                .ThenInclude(o => o.Product).FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.NumOrders = _context.Orders.Where(o => o.ClientId == order.ClientId).Count();

            return View(order);
        }
    }
}
