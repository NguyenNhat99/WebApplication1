// ViewModel/AdminProfileVM.cs
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModel
{
    public class AdminProfileVM
    {
            public int UserId { get; set; }
            public string Username { get; set; } = "";

            [Required, MaxLength(150)]
            public string FullName { get; set; } = "";

            [Required, EmailAddress, MaxLength(150)]
            public string Email { get; set; } = "";

            [Phone, MaxLength(20)]
            public string? Phone { get; set; }

            public string GroupName { get; set; } = "Admin";

    }

    public class ChangePasswordVM
    {
        public int UserId { get; set; }
        public string CurrentPassword { get; set; } = "";
        public string NewPassword { get; set; } = "";
    }
}
