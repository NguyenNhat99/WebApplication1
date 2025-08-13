namespace WebApplication1.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public int? UserId { get; set; }
        public User? User { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;
        public int TotalPrice { get; set; }
        public string Status { get; set; } = "Pending"; // Pending/Completed/Cancelled
        public string Address { get; set; } = ""; // Pending/Completed/Cancelled
        public string? Note { get; set; }

        public ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}
