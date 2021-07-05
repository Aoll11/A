using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace A.Models.Data
{
    // 11.
    [Table("tblProducts")] // Нужно для однозначной связи с таблицей из БД
    public class ProductDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string CategoryName { get; set; }
        public int CategoryId { get; set; }
        public string ImageName { get; set; }

        // Назначаем внешний ключ будем связывать c Id в CategoryDTO  имя ключа состоит из  CategoryID
        [ForeignKey("CategoryId")]
        public virtual CategoryDTO Category { get; set; }
    }
}