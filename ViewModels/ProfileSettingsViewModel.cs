using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SelcukDemo.ViewModels
{
    public class ProfileSettingsViewModel
    {
        [Required(ErrorMessage = "Doğum tarihi zorunludur.")]
        [DataType(DataType.Date)]
        [ValidBirthDate] // 👈 özel validasyonumuz
        public DateTime? BirthDate { get; set; }

        [Required(ErrorMessage = "Cinsiyet zorunludur.")]
        [Display(Name = "Cinsiyet")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Şehir zorunludur.")]
        [Display(Name = "Şehir")]
        public string City { get; set; }

        [Display(Name = "Profil Fotoğrafı")]
        public IFormFile? ProfileImage { get; set; }
    }
}