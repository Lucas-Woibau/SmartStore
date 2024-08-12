using Microsoft.AspNetCore.Mvc;
using SmarthStore.Models;
using SmarthStore.Services;

namespace SmarthStore.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly int _pageSzie = 5;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            this._context = context;
            this._environment = environment;
        }
        public IActionResult Index(int pageIndex, string? search, string ?column, string ?orderBy)
        {
            IQueryable<Product> query = _context.Products;

            //Search funcionality
            if(search != null)
            {
                query = query.Where(p => p.Name.Contains(search) || p.Brand.Contains(search));
            }

            //SortFuncionality
            string[] validColumns = { "Id", "Name", "Brand", "Category", "Price", "CreatedAt" };
            string[] validOrderBy = { "desc", "asc" };

            if (!validColumns.Contains(column))
            {
                column = "Id";
            }

            if (!validOrderBy.Contains(orderBy))
            {
                column = "desc";
            }

            query = query.OrderByDescending(p => p.Id);

            //Pagination funcionality
            if(pageIndex < 1)
            {
                pageIndex = 1;
            }

            decimal count = query.Count();
            int totalPages = (int)Math.Ceiling(count / _pageSzie);
            query = query.Skip((pageIndex - 1) * _pageSzie).Take(_pageSzie);

            var products = query.ToList();

            ViewData["PageIndex"] = pageIndex;
            ViewData["TotalPages"] = totalPages;

            ViewData["Search"] = search ?? "";

            ViewData["Column"] = column;
            ViewData["OrderBy"] = orderBy;

            return View(products);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(ProductDTO productDto)
        {
            if(productDto.ImageFile == null)
            {
                ModelState.AddModelError("ImageFile", "The image file is required");
            }

            if(!ModelState.IsValid)
            {
                return View(productDto);
            }

            //Save the image file
            string newFileName = DateTime.Now.ToString("yyyyMMddHHmmssff");
            newFileName += Path.GetExtension(productDto.ImageFile!.FileName);

            string imageFullPath = _environment.WebRootPath + "/products/" + newFileName;
            using(var stream = System.IO.File.Create(imageFullPath))
            {
                productDto.ImageFile.CopyTo(stream);
            }

            //Save the new product in the database
            Product product = new Product()
            {
                Name = productDto.Name,
                Brand = productDto.Brand,
                Category = productDto.Category,
                Price = productDto.Price,
                Description = productDto.Description,
                ImageFileName = newFileName,
                CreatedAt = DateTime.Now,
            };

            _context.Products.Add(product);
            _context.SaveChanges();

            return RedirectToAction("Index", "Products");
        }

        public IActionResult Edit(int id)
        {
            var product = _context.Products.Find(id);

            if(product == null)
            {
                return RedirectToAction("Index", "Products");
            }

            //Create productDto from product
            var productDto = new ProductDTO()
            {
                Name = product.Name,
                Brand = product.Brand,
                Category = product.Category,
                Price = product.Price,
                Description = product.Description,
            };

            ViewData["ProductId"] = product.Id;
            ViewData["ImageFileName"] = product.ImageFileName;
            ViewData["CreatedAt"] = product.CreatedAt.ToString("MM/dd/yyy");

            return View(productDto);
        }

        [HttpPost]
        public IActionResult Edit(int id, ProductDTO productDto)
        {
            var product = _context.Products.Find(id);

            if(product == null)
            {
                return RedirectToAction("Index", "Products");
            }

            if(!ModelState.IsValid)
            {
                ViewData["ProductId"] = product.Id;
                ViewData["ImageFileName"] = product.ImageFileName;
                ViewData["CreatedAt"] = product.CreatedAt.ToString("MM/dd/yyy");

                return View(productDto);
            }

            //Update the image file if we have a new image file
            string newFileName = product.ImageFileName;
            if(productDto.ImageFile != null)
            {
                newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                newFileName += Path.GetExtension(productDto.ImageFile.FileName);

                string imageFullPath = _environment.WebRootPath + "/products/" + newFileName;
                using (var stream = System.IO.File.Create(imageFullPath))
                {
                    productDto.ImageFile.CopyTo(stream);
                }

                //Delete the old image
                string oldImageFullPath = _environment.WebRootPath + "/products/" + product.ImageFileName;
                System.IO.File.Delete(oldImageFullPath);
            }

            //Update the product in the database
            product.Name = productDto.Name;
            product.Brand = productDto.Brand;
            product.Category = productDto.Category;
            product.Price = productDto.Price;
            product.Description = productDto.Description;
            product.ImageFileName = newFileName;

            _context.SaveChanges();

            return RedirectToAction("Index", "Products");
        }

        public IActionResult Delete(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
            {
                return RedirectToAction("Index", "Products");
            }

            string imageFullPath = _environment.WebRootPath + "/products/" + product.ImageFileName;
            System.IO.File.Delete(imageFullPath);

            _context.Products.Remove(product);
            _context.SaveChanges(true);

            return RedirectToAction("Index", "Products");
        }
    }
}
