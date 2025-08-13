using System.Text.Json.Serialization;

namespace WebApplication1.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string CategoryName { get; set; } = null!;
        [JsonIgnore]
        public ICollection<Food>? Foods { get; set; }
    }

}
