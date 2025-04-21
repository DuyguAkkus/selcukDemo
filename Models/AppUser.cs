using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SelcukDemo.Models
{
    public class AppUser : IdentityUser
    {
       
        public DateTime? BirthDate { get; set; }

      
        public string? Gender { get; set; }

   
        public string? City { get; set; }

        public string? ProfileImagePath { get; set; }

        // Kullanıcının kayıt olduğu tarih
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

}