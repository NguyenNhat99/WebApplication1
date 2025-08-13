using System.Text.Json.Serialization;

namespace WebApplication1.Models
{
    public class OrderDetail
    {
        public int OrderDetailId { get; set; }
        public int OrderId { get; set; }

        [JsonIgnore]                 // chặn OrderDetail -> Order -> OrderDetails -> ...
        public Order? Order { get; set; }

        public int FoodId { get; set; }
        public Food? Food { get; set; }

        public int Quantity { get; set; }
        public int Price { get; set; } // Đơn giá tại thời điểm đặt
    }
}
