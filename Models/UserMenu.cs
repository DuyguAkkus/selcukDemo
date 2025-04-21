namespace SelcukDemo.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
public class UserMenu
{
    
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } // Menü Adı (Örn: "Derslerim", "Form Listesi")

    public int? ParentId { get; set; } // Eğer alt menü ise, bağlı olduğu menünün ID'si

    public string? Area { get; set; }
    public string ControllerName { get; set; } // Controller Adı

    public string ActionName { get; set; } // Action Adı

    public string Icon { get; set; } // Menü simgesi (fa-solid fa-book gibi)

    public int SortNumber { get; set; } // Menü sırası

    public bool IsVisible { get; set; } = true; // Menü görünürlük durumu

    [ForeignKey("ParentId")]
    public virtual UserMenu ParentMenu { get; set; }

    // ❗ Yeni: Rol Seçmek İçin Kullanılacak Alan
    [NotMapped]
    public string SelectedRole { get; set; } // Kullanıcı bu rolü seçecek
}