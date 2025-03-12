using System.ComponentModel.DataAnnotations;

namespace SelcukDemo.ViewModels;

public class SignInViewModel_
{
    
    [Required(ErrorMessage = "Email gereklidir.")]
    [EmailAddress]
    public string Email { get; set; }

    [Required(ErrorMessage = "Şifre gereklidir.")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Display(Name = "Beni Hatırla")]
    public bool RememberMe { get; set; }
}