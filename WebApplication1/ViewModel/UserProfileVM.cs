﻿namespace WebApplication1.ViewModel
{
    public class UserProfileVM
    {
        public int UserId { get; set; }
        public string Username { get; set; } = "";
        public string FullName { get; set; } = "";
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }

}
