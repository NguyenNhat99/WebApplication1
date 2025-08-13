﻿namespace WebApplication1.ViewModel
{
    public class CartItemVM
    {
        public int FoodId { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        public decimal Total => Price * Quantity;
    }
}
