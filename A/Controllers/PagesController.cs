using A.Models.Data;
using A.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace A.Controllers
{
    public class PagesController : Controller
    {
        // 17.
        // GET: Index/{page}
        public ActionResult Index(string page = "")
        {
            // Получаем/устанавливаем краткий заголовок (Slug)
            if (page == "")
                page = "home";

            // Объявляем модель и класс DTO
            PageVM model;
            PagesDTO dto;

            // Проверяем, доступна ли страница
            using (Db db = new Db())
            {
                if (!db.Pages.Any(x => x.Slug.Equals(page)))
                    return RedirectToAction("Index", new { page = ""}); //Если не найдена страница со Slug, эквивалентным переданным в метод page, то вернёт на страницу Index
            }

            // Получаем DTO страницы
            using (Db db = new Db())
            {
                dto = db.Pages.Where(x => x.Slug == page).FirstOrDefault();
            }

            // Устанавливаем заголовки страницы (TITLE)
            ViewBag.PageTitle = dto.Title;

            // Проверяем боковую панель
            if(dto.HasSidebar == true)
            {
                ViewBag.Sidebar = "Yes";
            }
            else
            {
                ViewBag.Sidebar = "No";
            }

            // Заполняем модель данными
            model = new PageVM(dto);

            // Возвращаем представление вместе с моделью

                return View(model);
        }

        public ActionResult PagesMenuPartial()
        {
            // Инициализируем list PageVM
            List<PageVM> pageVMList;

            // Получаем все страницы, кроме HOME (на них ссылка отдельно)
            using (Db db = new Db())
            {
                pageVMList = db.Pages.ToArray().OrderBy(x => x.Sorting).Where(x => x.Slug != "home").Select(x => new PageVM(x)).ToList();
            }

            // Возвращаем частичное представление с листом данных          
            return PartialView("_PagesMenuPartial", pageVMList); // без _PagesMenuPartial не находит одноимённое частичное представление, так как оно на нижнее подчёркивание впереди отличается от метода, которому служит
        }

        public ActionResult SidebarPartial()
        {
            // Объявляем модель
            SidebarVM model;

            // Инициализировать модель данными
            using (Db db = new Db())
            {
                SidebarDTO dto = db.Sidebars.Find(1); // У нас пока лишь один SideBar

                model = new SidebarVM(dto);
            }

            // Возвращаем модель в частичное представление

            return  PartialView("_SidebarPartial", model);
        }
    }
}