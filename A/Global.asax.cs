using A.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace A
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        // 25. Создаём метод обработки запросов аутентификации.
        protected void Application_AuthenticateRequest()
        {
            // Проверяем, авторизован ли пользователь.
            if (User == null)
                return;

            // Получаем имя пользователя.
            string userName = Context.User.Identity.Name;

            // Объявляем массив ролей.
            string[] roles = null; //У юзера может быть несколько ролей!!! 

            using (Db db = new Db())
            {
                // Заполняем роли.
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == userName); //У юзера может быть несколько ролей!!! 

                if (dto == null)
                    return; // Во избежание ошибки при смене имени;

                roles = db.UserRoles.Where(x => x.UserId == dto.Id).Select(x => x.Role.Name).ToArray(); // Ролей может быть несколько.
            }

            // Создаём объект интерфейса IPrincipal
            IIdentity userIdentity = new GenericIdentity(userName); // Представляет универсального пользователя.
            IPrincipal newUserObj = new GenericPrincipal(userIdentity, roles);

            // Объявляем и инициализируем данными Context.User
            Context.User = newUserObj; //Теперь будет корректно получать данные для нашего пользователя - его роли и прочее.

        }

    }
}
