namespace SelcukDemo.Models;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


public class Lesson 
{
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public string TeacherId { get; set; } // Öğretmen ID'si

        [ForeignKey("TeacherId")]
        public virtual IdentityUser Teacher { get; set; } // Öğretmen bilgisi

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    
}