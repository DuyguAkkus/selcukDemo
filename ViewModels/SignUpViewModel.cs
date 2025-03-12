using System.ComponentModel.DataAnnotations;

namespace SelcukDemo.ViewModels;

public class SignUpViewModel
{
    [Required(ErrorMessage = "Kullanıcı adı gereklidir.")]
    public string UserName { get; set; }

    [Required(ErrorMessage = "Email gereklidir.")]
    [EmailAddress]
    public string Email { get; set; }

    [Required(ErrorMessage = "Telefon numarası gereklidir.")]
    [Phone]
    public string Phone { get; set; }

    [Required(ErrorMessage = "Şifre gereklidir.")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Required(ErrorMessage = "Şifre tekrar gereklidir.")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor!")]
    public string ConfirmPassword { get; set; }
}