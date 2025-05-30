namespace SelcukDemo.Areas.Admin.Models
{
    public class UserRoleViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }

        // Yeni eklenen alanlar
        public string? ProfileImagePath { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}