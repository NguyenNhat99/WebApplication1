namespace WebApplication1.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string Role { get; set; } = "Customer";
        public bool IsActive { get; set; } = true;
        public ICollection<Order>? Orders { get; set; }
    }

}
