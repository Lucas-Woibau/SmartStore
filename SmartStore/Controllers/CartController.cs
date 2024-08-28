using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartStore.Models;
using SmartStore.Services;

namespace SmartStore.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly decimal _shippingFee;

        public CartController(ApplicationDbContext context, IConfiguration configuration, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
            _shippingFee = configuration.GetValue<decimal>("CartSettings:ShippingFee");

        }
        public IActionResult Index()
        {
            List<OrderItem> cartItems = CartHelper.GetCartItems(Request, Response, _context);
            decimal subtotal = CartHelper.GetSubtotal(cartItems);

            ViewBag.CartItems = cartItems;
            ViewBag.Subtotal = subtotal;
            ViewBag.ShippingFee = _shippingFee;
            ViewBag.Total = subtotal + _shippingFee;

            return View();
        }
    }
}
