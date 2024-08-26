using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
            if (id == null)
            {
                return RedirectToAction("Index", "User");
            }

            var appUser = await _userManager.FindByIdAsync(id);

            if (appUser == null)
            {
                return RedirectToAction("Index", "User");
            }

            ViewBag.Roles = await _userManager.GetRolesAsync(appUser);

            // Get available roles
            var availableRoles = _roleManager.Roles.ToList();
            var items = new List<SelectListItem>();
            foreach (var role in availableRoles)
            {
                items.Add(
                    new SelectListItem
                    {
                        Text = role.NormalizedName,
                        Value = role.Name,
                        Selected = await _userManager.IsInRoleAsync(appUser, role.Name!),
                    });
            }

            ViewBag.SelectItems = items;

            return View(appUser);
        }

        public async Task<IActionResult> EditRole(string? id, string? newRole)
        {
            if (id == null || newRole == null)
            {
                return RedirectToAction("Index", "User");
            }

            var roleExists = await _roleManager.RoleExistsAsync(newRole);
            var appUser = await _userManager.FindByIdAsync(id);

            if (appUser == null || !roleExists)
            {
                return RedirectToAction("Index", "User");
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser!.Id == appUser.Id)
            {
                TempData["ErrorMessage"] = "You cannot update your own role!";
                return RedirectToAction("Details", "User", new { id });
            }

            // Update user role
            var userRoles = await _userManager.GetRolesAsync(appUser);
            await _userManager.RemoveFromRolesAsync(appUser, userRoles);
            await _userManager.AddToRoleAsync(appUser, newRole);

            TempData["SuccessMessage"] = "User Role updated successfully";
            return RedirectToAction("Details", "User", new { id });
        }

        public async Task<IActionResult> DeleteAccount(string? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "User");
            }

            var appUser = await _userManager.FindByIdAsync(id);

            if (appUser == null)
            {
                return RedirectToAction("Index", "User");
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser!.Id == appUser.Id)
            {
                TempData["ErrorMessage"] = "You cannot delete your own role!";
                return RedirectToAction("Details", "User", new { id });
            }

            // Delete
            var result = await _userManager.DeleteAsync(appUser);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "User");
            }

            TempData["ErrorMessage"] = "Unable to delete this account: " + result.Errors.First().Description;

            return RedirectToAction("Details", "User", new { id });
        }
    }
}
