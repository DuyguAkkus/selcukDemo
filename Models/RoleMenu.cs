namespace SelcukDemo.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
public class RoleMenu
{
    
    [Key]
    public int Id { get; set; }

    [Required]
    public string RoleName { get; set; } // Kullanıcı rolü (Örn: "Admin", "Student", "Teacher")

    [Required]
    public int MenuId { get; set; }

    [ForeignKey("MenuId")]
    public virtual UserMenu Menu { get; set; }

}