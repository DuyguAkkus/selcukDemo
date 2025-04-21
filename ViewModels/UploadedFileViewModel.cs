using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SelcukDemo.ViewModels
{
    public class UploadedFileViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Dosya adı gereklidir.")]
        public string FileName { get; set; }

        public string Description { get; set; }


        public IFormFile File { get; set; }

        [Required(ErrorMessage = "Kategori gereklidir.")]
        public string Category { get; set; }
    }
}