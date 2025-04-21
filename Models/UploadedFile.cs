using Microsoft.Build.Framework;

namespace SelcukDemo.Models;

public class UploadedFile
{
        public int Id { get; set; }

        [Required] public string FileName { get; set; }

        public string Description { get; set; }

        [Required] public string FilePath { get; set; }

        [Required] public string Category { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.Now;
    
}