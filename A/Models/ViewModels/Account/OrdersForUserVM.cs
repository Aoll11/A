using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace A.Models.ViewModels.Account
{
    public class OrdersForUserVM
    {
        [DisplayName("Order Number")]
        public int OrderNumber { get; set; }

        public decimal Total { get; set; }  //Цена

        public Dictionary<string, int> ProductsAndQty { get; set; } // Название и количество товара

        [DisplayName("Created At")]
        public DateTime CreatedAt { get; set; }
    }
}