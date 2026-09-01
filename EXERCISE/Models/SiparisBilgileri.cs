using System.ComponentModel.DataAnnotations;

namespace EXERCISE_MVC.Models
{
    public class SiparisBilgileri
    {
        [Required(ErrorMessage = "Ad zorunludur")]
        [StringLength(50, MinimumLength = 2)]
        public string? Ad { get; set; }

        [Required(ErrorMessage = "Soyad zorunludur")]
        [StringLength(50, MinimumLength = 2)]
        public string? Soyad { get; set; }

        [Required(ErrorMessage = "Adres zorunludur")]
        [StringLength(200, MinimumLength = 10)]
        public string? Adres { get; set; }

        [Required(ErrorMessage = "Telefon zorunludur")]
        [Phone(ErrorMessage = "Geçerli telefon numarası giriniz")]
        public string? Telefon { get; set; }
    }
}