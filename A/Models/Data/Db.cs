using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace A.Models.Data
{
    public class Db:DbContext

    {
        public DbSet<PagesDTO> Pages { get; set; } //будем загребать данные по структуре соответствующие PagesDTO и в ней же указано, из какой именно таблицы -    [Table("tblPages")]
        
        // 6.
        public DbSet<SidebarDTO> Sidebars { get; set; }
        
        // 8.
        public DbSet<CategoryDTO> Categories { get; set; }
        
        // 11.
        public DbSet<ProductDTO> Products { get; set; }
        
        // 22.
        public DbSet<UserDTO> Users { get; set; }

        public DbSet<RoleDTO> Roles { get; set; }
        
        // 23.
        public DbSet<UserRoleDTO> UserRoles { get; set; }
        
        // 25.
        public DbSet<OrderDTO> Orders { get; set; }

        public DbSet<OrderDetailsDTO> OrderDetails { get; set; }





    }
}