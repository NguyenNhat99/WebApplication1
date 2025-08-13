// ViewModels/Food/FoodFormVM.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace WebApplication1.ViewModels.Food
{
    public class FoodFormVM
    {
        public int? FoodId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        public string Name { get; set; } = "";

        [Range(1, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        public int? CategoryId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mô tả")]
        public string Description { get; set; } = "";

        public bool IsActive { get; set; } = true;

        // Upload
        public IFormFile? Avatar { get; set; }
        public bool RemoveImage { get; set; }

        // Hiển thị
        public string ImageUrl { get; set; } = "/assets/img/no-image.png";

        // Danh mục để đổ dropdown
        public List<Common.CategoryOption> CategoryOptions { get; set; } = new();
    }

    namespace Common
    {
        public class CategoryOption
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
        }
    }
}
