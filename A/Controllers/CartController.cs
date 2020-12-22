
using A.Models.Cart;
using A.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
// using System.Net.Mail.SmtpClient; но так выдаёт ошибку, утверждает, что это лишь тип, а не пространство имён.
using System.Web;
using System.Web.Mvc;

namespace A.Controllers
{
    public class CartController : Controller
    {
        // 20.
        // GET: Cart
        public ActionResult Index()
        {
            // Объявляем лист типа CartVM.
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            // Проверяем не пустая ли корзина.
            if (cart.Count == 0 || Session["cart"] == null)
            {
                ViewBag.Message = "Your cart is empty.";
                return View();
            }

            // Складываем сумму и записываем во ViewBag.
            decimal total = 0m;

            foreach (var item in cart)
            {
                total += item.Total;
            }

            ViewBag.GrandTotal = total;

            // Возвращаем лист в представление.
            return View(cart);
        }

        public ActionResult CartPartial()
        {
            // Объявляем модель CartVM
            CartVM model = new CartVM();

            // Объявляем переменную количества
            int qty = 0;

            // Объявляем переменную цены
            decimal price = 0m;

            // Проверяем сессию корзины. Имеются ли в ней данные
            if (Session["cart"] != null) 
            {
                // Получаем общее количество товаров и цену. 
                var list = (List<CartVM>)Session["cart"];
                foreach (var item in list)
                {
                    qty += item.Quantity;
                    price += item.Quantity * item.Price; 
                }

                model.Quantity = qty;
                model.Price = price;

            }
            else
            {
                // Или устанавливаем количество и цену в 0.
                model.Quantity = 0;
                model.Price = 0m;
            }

            // Возвращаем частичное представление с моделью.
            return PartialView("_CartPartial", model);
        }

        // 21.
        public ActionResult AddToCartPartial(int id)
        {
            // Объявляем лист параметризированный CartVM.
            List<CartVM> cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            // Объявляем модель CartVM.
            CartVM model = new CartVM();

            using (Db db = new Db())
            {
                // Получаем товар по Id.
                ProductDTO product = db.Products.Find(id);

                // Проверяем находится ли уже товар в корзине.
                var productInCart = cart.FirstOrDefault(x => x.ProductId == id);

                // Если нет, то добавляем этот товар.
                if (productInCart == null)
                {
                    cart.Add(new CartVM()
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = 1,
                        Price = product.Price,
                        Image = product.ImageName
                    });
                }
                // Если да, то добавляем единицу товара.
                else
                {
                    productInCart.Quantity++;
                }
            }

            // Получаем общее колличество, цену и добавляем данные в модель.
            int qty = 0;
            decimal price = 0m;

            foreach (var item in cart)
            {
                qty += item.Quantity;
                price += item.Quantity * item.Price;
            }

            model.Quantity = qty;
            model.Price = price;

            // Сохраняем состояние корзины в сессию.
            Session["cart"] = cart;

            // Возвращаем частичное представление с моделью. 
            return PartialView("_AddToCartPartial", model);
        }

        // 21.
        // GET: /cart/IncrementProduct 
        public JsonResult IncrementProduct(int productId)
        {
            // Объявить лист cart.
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                // Получаем CartVM из листа.
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                // Добавляем количество.
                model.Quantity++;

                // Сохраняем необходимые данные
                var result = new { qty = model.Quantity, price = model.Price };

                // Возвращаем JSON ответ с данными.

                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        // 22.
        // GET: /cart/DecrementProduct
        public ActionResult DecrementProduct(int productId)
        {
            // Объявить лист cart.
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                // Получаем CartVM из листа.
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                // Отнимаем количество.
                if (model.Quantity > 1)
                    model.Quantity--;
                else
                {
                    model.Quantity = 0;
                    cart.Remove(model);
                }

                // Сохраняем необходимые данные
                var result = new { qty = model.Quantity, price = model.Price };

                // Возвращаем JSON ответ с данными.

                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        // 22.
        // GET: /cart/RemoveProduct
        public void RemoveProduct(int productId)
        {
            // Объявить лист cart.
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                // Получаем CartVM из листа.
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                cart.Remove(model);
            }
        }

        // 26.
        public ActionResult PaypalPartial()
        {
            // Получить список товаров в корзине.
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            // Вернуть частичное представление вместе с этим списком товаров.
            return PartialView(cart);
        }

        // 26.
        // POST: /cart/PlaceOrder
        [HttpPost]
        public void PlaceOrder()
        {
            // Получить список товаров в корзине.
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            // Получаем имя пользователя.
            string userName = User.Identity.Name;

            // Объявить переменную для orderId
            int orderId = 0;

            using (Db db = new Db())
            {
                // Объявляем модель OrderDTO
                OrderDTO orderDto = new OrderDTO();

                // Получаем Id пользователя.
                var q = db.Users.FirstOrDefault(x => x.Username == userName);
                int userId = q.Id;

                // Заполняем модель  OrderDTO данными и сохраняем её.
                orderDto.UserId = userId;
                orderDto.CreatedAt = DateTime.Now;

                db.Orders.Add(orderDto);
                db.SaveChanges();

                // Получаем orderId
                orderId = orderDto.OrderId;

                // Объявляем модель OrderDetailsDTO
                OrderDetailsDTO orderDetailsDTO = new OrderDetailsDTO();

                // Добавляем в модель данные.
                foreach (var item in cart)
                {
                    orderDetailsDTO.OrderId = orderId;
                    orderDetailsDTO.UserId = userId;
                    orderDetailsDTO.ProductId = item.ProductId;
                    orderDetailsDTO.Quantity = item.Quantity;

                    db.OrderDetails.Add(orderDetailsDTO);
                    db.SaveChanges();
                }
            }
            // Отправляем письмо о заказе на почту администратора. 
            var client = new SmtpClient("smtp.mailtrap.io", 2525)
            {
                Credentials = new NetworkCredential("b308869a652127", "1c658ae23e9618"),  // ("username", "password")
                EnableSsl = true //включение шифрования данных.
            };
            client.Send("shop@example.com", "admin@example.com", "New order", $"You have a new order. Order number: {orderId}");

            // Обнуляем сессию !!! Иначе возможны ошибки с paypal, в том числе и в следующих сессиях!

            Session["cart"] = null;
            // return View(); //ничего не возвращаем.
        }



    }
}