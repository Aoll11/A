using A.Models.Data;
using A.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace A.Controllers
{
    public class ShopController : Controller
    {
        // 18
        // GET: Shop
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Pages");
        }

        public ActionResult CategoryMenuPartial()
        {
            // Объявляем модель типа List<> CategoryVM 
            List<CategoryVM> categoryVMList;

            // Инициализировать модель данными
            using (Db db = new Db())
            {
                categoryVMList = db.Categories.ToArray().OrderBy(x => x.Sorting).Select(x => new CategoryVM(x)).ToList();
            }

            // Возвращаем частичное представление с моделью
            return PartialView("_CategoryMenuPartial", categoryVMList);
        }

        // 19 Метод вывода товаров по категориям.
        // GET: Shop/Category/name
        public ActionResult Category(string name)
        {
            // Объявляем список типа List
            List<ProductVM> productVMList;

            // Получаем Id категории
            using (Db db = new Db())
            {
                CategoryDTO categoryDTO = db.Categories.Where(x => x.Slug == name).FirstOrDefault();

                int catId = categoryDTO.Id;

                // Инициализируем наш список данными
                productVMList = db.Products.ToArray().Where(x => x.CategoryId == catId).Select(x => new ProductVM(x)).ToList() ;

                // Получаем имя категории (на деле - первый, содержащий нужную категорию ProductDTO) Переделать в версии с рядом категорий для одного товара или основная категория одна, а остальные - дополнительные
                var productCat = db.Products.Where(x => x.CategoryId == catId).FirstOrDefault();

                // Делаем проверку на Null
                if (productCat == null)
                {
                    var catName = db.Categories.Where(x => x.Slug == name).Select(x => x.Name).FirstOrDefault();
                    ViewBag.CategoryName = catName;
                }
                else
                {
                    ViewBag.CategoryName = productCat.CategoryName;
                }
            }
            
            // Возвращаем представление с моделью.
            return View(productVMList);
        }

        // 19 Карточка продукта.
        // GET: Shop/product-details/name
        [ActionName("product-details")]
        public ActionResult ProductDetails(string name)
        {
            // Объявляем 2 модели DTO и VM
            ProductDTO dto;
            ProductVM model;

            // Инициализируем Id продукта
            int id = 0;
           
            using (Db db = new Db())
            {
                // Проверяем, доступен ли продукт
                if (!db.Products.Any(x => x.Slug.Equals(name))) 
                {
                    return RedirectToAction("Index", "Shop"); 
                }
                // Инициализируем модель DTO данными
                dto = db.Products.Where(x => x.Slug == name).FirstOrDefault();

                // Получаем Id
                id = dto.Id;

                // Инициализируем модель VM данными
                model = new ProductVM(dto);
            }

            // Получаем изображения из галлерие
            model.GalleryImages = Directory
                .EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                .Select(fn => Path.GetFileName(fn));

            // Возвращаем модель в представление

            return View("ProductDetails", model);
        }
    }
}