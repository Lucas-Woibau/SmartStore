using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartStore.Models;
using System.Linq;

namespace SmartStore.Controllers
{
    [Authorize(Roles = "admin")]
    [Route("/Admin/[controller]/{action=Index}/{id?}")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly int _pageSize = 5;
        public UserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public IActionResult Index(int? pageIndex)
        {
            IQueryable<ApplicationUser> query = _userManager.Users.OrderByDescending(u => u.CreatedAt);

            if (pageIndex == null || pageIndex < 1)
            {
                pageIndex = 1;
            }

            decimal count = query.Count();
            int totalPages = (int)Math.Ceiling(count / _pageSize);
            query = query.Skip(((int)pageIndex - 1) * _pageSize).Take(_pageSize);

            var users = query.ToList();

            ViewBag.PageIndex = pageIndex;
            ViewBag.TotalPages = totalPages;

            return View(users);
        }

        public async Task<IActionResult> Details(string? id)
        {
            if(id == null)
            {
                return RedirectToAction("Index", "Users");
            }

            var appUser = await _userManager.FindByIdAsync(id);

            if (appUser == null)
            {
                return RedirectToAction("Index", "Users");
            }

            ViewBag.Roles = await _userManager.GetRolesAsync(appUser);

            return View(appUser);
        }

    }
}
