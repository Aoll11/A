using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace A.Areas.Admin.Models.ViewModels.Shop
{
    public class OrdersForAdminVM
    {
        [DisplayName("Order Number")]
        public int OrderNumber { get; set; }

        [DisplayName("Username")]
        public string UserName { get; set; }

        public decimal Total { get; set; }  //Цена
        public Dictionary<string, int> ProductsAndQty { get; set; } // Название и количество товара

        [DisplayName("Created At")]
        public DateTime CreatedAt { get; set; }
    }
}