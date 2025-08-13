// ViewModels/Home/HomeIndexVM.cs
using System.Collections.Generic;

namespace WebApplication1.ViewModels.Home
{
    public class HomeIndexVM
    {
        public List<CategoryVM> Categories { get; set; } = new();
        public List<FoodCardVM> Foods { get; set; } = new();

        // filter/search/paging (tuỳ cần)
        public int? CategoryId { get; set; }
        public string? Query { get; set; }
        public int Page { get; set; } = 1;
        public int NoOfPages { get; set; } = 1;
    }

    public class CategoryVM
    {
        public int Id { get; set; }
        public string CategoryName { get; set; } = "";
    }

    public class FoodCardVM
    {
        public int FoodId { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = "/assets/img/no-image.png";
        public int? CategoryId { get; set; }
        public bool IsActive { get; set; }
    }
}
