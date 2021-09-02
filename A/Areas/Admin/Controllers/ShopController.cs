using A.Areas.Admin.Models.ViewModels.Shop;
using A.Models.Data;
using A.Models.ViewModels.Shop;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace A.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]

    public class ShopController : Controller
    {
        // GET: Admin/Shop
        public ActionResult Categories()
        {
            //Объявляем модель типа List
            List<CategoryVM> categoryVMList;

            using (Db db = new Db())
            {
                //Инициализируем модель данными
                categoryVMList = db.Categories
                                    .ToArray()
                                    .OrderBy(x => x.Sorting)
                                    .Select(x => new CategoryVM(x))
                                    .ToList();
            }
                //Возвращаем List в представление.

                return View(categoryVMList);
        }

        //9. GET отсутствует, так как обращение идёт из Ajax в Categories
        // POST: Admin/Shop/AddNewCategory
        [HttpPost]
        public string AddNewCategory(string catName)
        {
            // Объявляем строкову переменную id
            string id;

            using (Db db = new Db())
            {
                // Проверяем имя категории на уникальность.
                if (db.Categories.Any(x => x.Name == catName))  //если есть любая, такая категория, имя которой равняется содержимому catName
                    return "titletaken";

                // Инициализируем модель DTO
                CategoryDTO dto = new CategoryDTO();

                // Добавляем данные в модель
                dto.Name = catName;
                dto.Slug = catName.Replace(" ", "-").ToLower();
                dto.Sorting = 100;

                // Сохраняем
                db.Categories.Add(dto);
                db.SaveChanges();

                // Получаем ID для возврата в представление
                id = dto.Id.ToString();

            }

            // Возвращаем ID в представление
            return id;

        }

        // 9. Создаём метод сортировки.
        // POST: Admin/Shop/ReorderCategories
        [HttpPost]
        public void ReorderCategories(int[] id)
        {
            using (Db db = new Db())
            {
                //Реализуем начальный счётчик
                int count = 1;

                //Иинициализируем модель данных
                CategoryDTO dto;

                //Устанавливаем сортировку для каждой страницы
                foreach (var catId in id)
                {
                    dto = db.Categories.Find(catId);
                    dto.Sorting = count;

                    db.SaveChanges();
                    count++;
                }

            }



        }

        // 9. Удаление страницы 
        // GET: Admin/Shop/DeleteCategory/id
        public ActionResult DeleteCategory(int id)
        {
            using (Db db = new Db())
            {
                //Получаем модель категории.
                CategoryDTO dto = db.Categories.Find(id);

                //Удаляем категорию.
                db.Categories.Remove(dto);

                //Сохраняем изменения в базе.
                db.SaveChanges();
            }

            //Добавляем сообщение об успешном удалении категории.
            TempData["SM"] = "You have deleted a category!";

            //Переадресовка пользователя.

            return RedirectToAction("Categories");
        }

        // 10. Удаление страницы 
        // POST: Admin/Shop/RenameCategory/id
        [HttpPost]
        public string RenameCategory(string newCatName, int id)
        {
            using (Db db = new Db())
            {
                // Проверяем имя на уникальность.
                if (db.Categories.Any(x => x.Name == newCatName))
                    return "titletaken";

                // Получаем модель DTO.
                CategoryDTO dto = db.Categories.Find(id);

                // Редактируем модель DTO.
                dto.Name = newCatName;
                dto.Slug = newCatName.Replace(" ", "-").ToLower();

                // Сохраняем изменения.
                db.SaveChanges();
            }
            // Возвращаем слово (может понадобиться дальше, не сейчас)
            return "ok";
        }

        // 11. Метод добавления товара
        // GET: Admin/Shop/AddProduct
        [HttpGet]
        public ActionResult AddProduct()
        {
            //Объявляем модель данных
            ProductVM model = new ProductVM();

            //Добавляем список категорий из базы в модель
            using(Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "id", "name"); //приводим к категории List "id" and "name"
            }

            //Возвращаем модель в представление

            return View(model);
        }

        // 12. Метод добавления товара
        // POST: Admin/Shop/AddProduct
        [HttpPost]
        public ActionResult AddProduct(ProductVM model, HttpPostedFileBase file ) //HttpPostedFileBase - базовый класс предоставляющий доступ к файлам загруженным на сервер
        {
            // Проверяем модель на валидность
            if(!ModelState.IsValid)
            {
                using (Db db = new Db())
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name"); // по новой заполняем список категорий, иначе он останется в модели пустым и мы получим ошибку
                    return View(model);
                }
            }

            // Проверяем имя продукта на уникальность
            using (Db db = new Db())
            {
                if(db.Products.Any(x=>x.Name==model.Name))
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name"); // по новой заполняем список категорий, иначе он останется в модели пустым и мы получим ошибку
                    ModelState.AddModelError("", "That product name is taken!");
                    return View(model);
                }
            }

            // Объявляем переменную ProductId
            int id;

            // Инициализируем и сохраняем модель на основе ProductDTO
            using (Db db = new Db())
            {
                ProductDTO product = new ProductDTO();

                product.Name = model.Name;
                product.Slug = model.Name.Replace(" ", "-").ToLower();
                product.Description = model.Description;
                product.Price = model.Price;
                product.CategoryId = model.CategoryId;

                //Модель категорий
                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                product.CategoryName = catDTO.Name;

                db.Products.Add(product);
                db.SaveChanges();

                id = product.Id;
            }

            // Добавляем сообщение в TempData
            TempData["SM"] = "You have added a product!";


            //Регион - без острой необходимости лучше не пользоваться. Тут добален для сворачивания, чтобы не мешал логике основного кода. 

            #region Upload Image

            // Создать необходимые ссылки директорий, формируем пути
            var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads")); //Длинен путь - имена длинные хранить не желает. Но пока так.
            //var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Img\\Upl"));


            var pathString1 = Path.Combine(originalDirectory.ToString(), "Products" );  //объединять массив строк в путь.
            var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
            var pathString3 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs"); //Thumbs - уменьшенные копии картинок
            var pathString4 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
            var pathString5 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

            // Проверяем наличие директорий (Если нет - создаём)
            if (!Directory.Exists(pathString1))
                Directory.CreateDirectory(pathString1);

            if (!Directory.Exists(pathString2))
                Directory.CreateDirectory(pathString2);

            if (!Directory.Exists(pathString3))
                Directory.CreateDirectory(pathString3);

            if (!Directory.Exists(pathString4))
                Directory.CreateDirectory(pathString4);

            if (!Directory.Exists(pathString5))
                Directory.CreateDirectory(pathString5);

            // Проверяем, был ли файл загружен
            if (file != null && file.ContentLength > 0)
            {
                // Получаем расширение файла.
                string ext = file.ContentType.ToLower();

                // Проверяем расширение файла.
                if (ext != "image/jpg" &&
                   ext != "image/jpeg" &&
                   ext != "image/pjpeg" &&
                   ext != "image/gif" &&
                   ext != "image/x-png" &&
                   ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                        ModelState.AddModelError("", "The image was not uploaded - wrong image extension");
                        return View(model);
                    }
                }

                // Объявляем переменную с именем изображения
                string imageName = file.FileName;

                // Проверяем длинну имени, дабы исключить ошибку от слишком длинных имён. 
                int AllowedImgNameLength = 120; // допустимая длина имени 
                if(imageName.Length > AllowedImgNameLength)
                // Укорачиваем имя до допустимой длины.
                {
                    imageName = imageName.Remove(AllowedImgNameLength - 16, imageName.Length - AllowedImgNameLength); // 16 - символов для номера фото (находится в конце) и расширение файла
                }
                
                // Сохраняем имя изображения в модель DTO 
                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;

                    db.SaveChanges();
                }

                // Назначаем пути к оргигинальному и уменьшенному изображению
                var path = string.Format($"{pathString2}\\{imageName}");
                var path2 = string.Format($"{pathString3}\\{imageName}");

                // Сохраняем оригинальное изображение
                file.SaveAs(path);

                // Создаём и сохраняем уменьшенную копию
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200).Crop(1,1);
                img.Save(path2);
                    
            }

            #endregion

            // Переадресовываем пользователя

            return RedirectToAction("AddProduct");
        }

        // 13. Метод списка товаров
        // Get: Admin/Shop/Products
        [HttpGet]
        public ActionResult Products(int? page, int? catId)
        {
            // Объявляем ProductVM  типа List
            List<ProductVM> listOfProductVM;

            // Устанавливаем номер страницы (код из официальной доккументации к PagedList.Mvc)
            var pageNumber = page ?? 1; // Если page == null, то присвоить pageNumber значение 1

            using (Db db = new Db())
            {
                // Инициализируем list и заполняем данными
                listOfProductVM = db.Products.ToArray()
                    .Where(x => catId == null || catId == 0 || x.CategoryId == catId) //  catId == null || catId == 0 не накладывают никаких ограничений на список, так как не связаны с передаваемыми по очереди в were элементами.. Т.е. если catId таков, то гребём всё.
                    .Select(x => new ProductVM(x))
                    .ToList();

                // Заполняем категории данными для сортировки
                ViewBag.Categories = new SelectList(db.Categories.ToList(), "id", "Name");

                // Устанавливаем выбранную категорию
                ViewBag.selectedCat = catId.ToString();
            }

            // Устанавливаем постраничную навигацию
            var onePageOfProducts = listOfProductVM.ToPagedList(pageNumber, 3); // номер страницы, сколько товаров на странице (рекомендуется 25)
            ViewBag.onePageOfProducts = onePageOfProducts;

            // Возвращаем представление с данными
            return View(listOfProductVM);
        }

        // 14. Метод редактирования товара
        // GET: Admin/Shop/EditProduct/id
        [HttpGet]
        public ActionResult EditProduct(int id)
        {
            // Объявить модель ProductVM
            ProductVM model;

            using (Db db = new Db())
            {
                // Получаем продукт
                ProductDTO dto = db.Products.Find(id);

                // Проверяем доступнен ли продукт
                if (dto == null)
                {
                    return Content("That product does not exist."); //вернёт строку 
                }

                // Инициализируем модель данными
                model = new ProductVM(dto);

                // Создаём список категорий
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                // Получаем все изображения из галереи
                model.GalleryImages = Directory
                    .EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                    .Select(fn => Path.GetFileName(fn));
            }
            // Возвращаем модель в представление            
            return View(model);
        }

        // 14-15. Метод редактирования товара
        // POST: Admin/Shop/EditProduct
        [HttpPost ]
        public ActionResult EditProduct(ProductVM model, HttpPostedFileBase file) //  HttpPostedFileBase file  - картинка, имя должно совпадать с name, в этом представлении:  <input type="file" , name="file" id="imageUpload" /> 
        {
            // Получаем Id продукта
            int id = model.Id;

            // Заполняем список категориями и изображениями
            using(Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }

            model.GalleryImages = Directory
                    .EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                    .Select(fn => Path.GetFileName(fn));

            // Проверяем модель на валидность
            if(!ModelState.IsValid)
            {
                return View(model);
            }

            // Проверяем имя продукта на уникальность
            using (Db db = new Db())
            {
                if(db.Products.Where(x=>x.Id!=id).Any(x=>x.Name==model.Name))
                {
                    ModelState.AddModelError("", "That product name is taken!");
                    return View(model);
                }
            }

            // Обновляем продукт в базе данных
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);

                dto.Name = model.Name;
                dto.Slug = model.Name.Replace(" ", "-").ToLower();
                dto.Description = model.Description;
                dto.Price = model.Price;
                dto.CategoryId = model.CategoryId;
                dto.ImageName = model.ImageName;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                dto.CategoryName = catDTO.Name;

                db.SaveChanges();
            }

            // Устанавливаем сообщение в TempData
            TempData["SM"] = "You have edited the product!";

            // Логика обработки изображений (15)
            #region Image Upload

            // Проверяем, загружен ли файл?
            if (file != null && file.ContentLength > 0)
            {
                // Получаем расширение файла.
                string ext = file.ContentType.ToLower();

                // Проверяем расширение файла.
                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        ModelState.AddModelError("", "The image was not uploaded - wrong image extension");
                        return View(model);
                    }
                }


                // Устанавливаем пути для загрузки
                var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));

                var pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
                var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs"); //Thumbs - уменьшенные копии картинок

                // Удаляем существующие файлы в директориях и директории
                DirectoryInfo di1 = new DirectoryInfo(pathString1);
                DirectoryInfo di2 = new DirectoryInfo(pathString2);

                foreach (var file2 in di1.GetFiles())
                {
                    file2.Delete();
                }

                foreach (var file3 in di2.GetFiles())
                {
                    file3.Delete();
                }

                // Сохраняем имя изображения
                string imageName = file.FileName;

                // Проверяем длинну имени, дабы исключить ошибку от слишком длинных имён. 
                int AllowedImgNameLength = 120; // допустимая длина имени 

                // Если длиннее - укорачиваем имя до допустимой длины.
                if (imageName.Length > AllowedImgNameLength)
                {
                    imageName = imageName.Remove(AllowedImgNameLength - 16, imageName.Length - AllowedImgNameLength); // 16 - символов для номера фото (находится в конце) и расширение файла
                }


                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;
                    db.SaveChanges();
                }

                // Сохраняем оригинал и превью версии

                var path = string.Format($"{pathString1}\\{imageName}");
                var path2 = string.Format($"{pathString2}\\{imageName}");

                // Сохраняем оригинальное изображение
                file.SaveAs(path);

                // Создаём и сохраняем уменьшенную копию
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200).Crop(1,1);
                img.Save(path2);                
            }
            
            #endregion

            // Переадресовываем пользователя 
            return RedirectToAction("EditProduct");
        }

        // 15. Метод удаления товара.
        // GET: Admin/Shop/DeleteProduct/id
        [HttpGet]
        public ActionResult DeleteProduct(int id)
        {
            // Удаляем товар из базы данных.
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                db.Products.Remove(dto);

                db.SaveChanges();
            }

            // Удаляем дирректории товара (изображения)
            var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));
            var pathString = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());

            if (Directory.Exists(pathString))
                Directory.Delete(pathString, true); // Удаление как каталога, так и подкаталогов
            
            // Переадресуем пользователя.
            return RedirectToAction("Products");
        }

        // 16. Метод добавления изображений в галерею.
        // POST: Admin/Shop/SaveGalleryImages/id
        [HttpPost]
        public void SaveGalleryImages(int id)
        {
            // Перебираем все полученные файлы
            foreach (string filename in Request.Files)
            {
                // Инициализируем файлы
                HttpPostedFileBase file = Request.Files[filename];

                // Проверяем на Null
                if (file != null && file.ContentLength > 0)
                {
                    // Получаем расширение файла.
                    string ext = file.ContentType.ToLower();

                    // Проверяем расширение файла. DropzoneJS пропускает .webp, однако его не принимает WebImage. Потому таковые останутся без иконки. 
                    // А не имея иконок не попадут в выдачу, хотя и останутся записанными.
                    if (ext != "image/jpg" &&
                        ext != "image/jpeg" &&
                        ext != "image/pjpeg" &&
                        ext != "image/gif" &&
                        ext != "image/x-png" &&
                        ext != "image/png")
                    {
                        // Выдать пустую тоже не можем, поскольку сторонний плагин для пролистования изображений .webp тоже не видит. Потому просто пропускаем его.
                        //WebImage img = new WebImage("~/Content/img/no_image.png");
                        //img.Resize(200, 200).Crop(1, 1);
                        //img.Save(path2);
                        continue;
                    }
                                 
                    // Назначаем все пути к директориям
                    var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));
                    string pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
                    string pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");


                    // Объявляем переменную с именем изображения
                    string imageName = file.FileName;

                    // Проверяем длинну имени, дабы исключить ошибку от слишком длинных имён. 
                    int AllowedImgNameLength = 120; // допустимая длина имени 

                    // Если длиннее, то укорачиваем имя до допустимой длины.
                    if (imageName.Length > AllowedImgNameLength)
                    {
                        imageName = imageName.Remove(AllowedImgNameLength - 16, imageName.Length - AllowedImgNameLength); // 16 - символов для номера фото (находится в конце) и расширение файла
                    }

                    // Назначаем пути изображений
                    var path = string.Format($"{pathString1}\\{imageName}");
                    var path2 = string.Format($"{pathString2}\\{imageName}");

                    // Сохраняем оригинальные и уменьшенные копии
                    file.SaveAs(path);

                    WebImage img = new WebImage(file.InputStream);
                    img.Resize(200, 200).Crop(1, 1);
                    img.Save(path2);
                }
            }

        }

        // 16. Метод удаления изображений из галереи.
        // POST: Admin/Shop/DeleteImage/id/imageName
        [HttpPost]
        public void DeleteImage(int id, string imageName)
        {
            string fullPath1 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/" + imageName);
            string fullPath2 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/Thumbs/" + imageName);

            if (System.IO.File.Exists(fullPath1))
                System.IO.File.Delete(fullPath1);

            if (System.IO.File.Exists(fullPath2))
                System.IO.File.Delete(fullPath2);

        }

        // 27. Метод вывода всех заказов для администратора .
        // GET: Admin/Shop/Orders
        public ActionResult Orders()
        {
            // Инициализируем модель OrdersForAdminVM
            List<OrdersForAdminVM> ordersForAdmin = new List<OrdersForAdminVM>();

            using (Db db = new Db())
            {
                // Инициализируем модель OrderVM
                List<OrderVM> orders = db.Orders.ToArray().Select(x => new OrderVM(x)).ToList();

                // Перебираем данные модели OrderVM
                foreach (var order in orders)
                {
                    // Инициализируем словарь товаров.
                    Dictionary<string, int> productAndQty = new Dictionary<string, int>();

                    // Объявляем переменную общей суммы.
                    decimal total = 0m; //

                    // Инициализируем лист OrderDetailsDTO
                    List<OrderDetailsDTO> orderDetailsList = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

                    // Получаем имя пользователя.
                    UserDTO user = db.Users.FirstOrDefault(x => x.Id == order.UserId);
                    string username = user.Username;

                    // Перебираем список товаров из OrderDetailsDTO
                    foreach (var orderDetails in orderDetailsList)
                    {
                        // Получаем товар. 
                        ProductDTO product = db.Products.FirstOrDefault(x => x.Id == orderDetails.ProductId);

                        // Получаем цену товара.
                        decimal price = product.Price;

                        // Получаем название товара.
                        string productName = product.Name;

                        // Добавляем товар в словарь.
                        productAndQty.Add(productName, orderDetails.Quantity);

                        // Получаем полную стоимость товаров.
                        total += orderDetails.Quantity * price;
                    }
                    // Добавляем данные в модель OrdersForAdminVM.
                    ordersForAdmin.Add(new OrdersForAdminVM()
                    {
                        OrderNumber = order.OrderId,
                        UserName = username,
                        Total = total,
                        ProductsAndQty = productAndQty,
                        CreatedAt = order.CreatedAt
                    });
                }
            }
            // Возрващаем представление вместе с моделью OrdersForAdminVM.
            return View(ordersForAdmin);
        }

    }
}