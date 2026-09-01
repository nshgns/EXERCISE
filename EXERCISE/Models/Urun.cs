using System.ComponentModel.DataAnnotations;
namespace EXERCISE_MVC.Models
{
    public class Urun
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ürün adı zorunludur")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Ürün adı 3-100 karakter arasında olmalıdır")]
        public string? Ad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Fiyat zorunludur")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Fiyat 0'dan büyük olmalıdır")]
        public decimal Fiyat { get; set; }

        [Required(ErrorMessage = "Kategori zorunludur")]
        [StringLength(50, ErrorMessage = "Kategori maksimum 50 karakter olmalıdır")]
        public string Kategori { get; set; } = "";

        public string? ResimYolu { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stok negatif olamaz")]
        public int Stok { get; set; }
    }
}