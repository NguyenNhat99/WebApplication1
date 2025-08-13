namespace WebApplication1.Models
{
    public class Food
    {
        public int FoodId { get; set; }
        public string Name { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
        public string? Description { get; set; }
        public int Price { get; set; }
        public bool IsActive { get; set; } = true;
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }
    }

}
