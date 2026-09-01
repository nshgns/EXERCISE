using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EXERCISE_MVC.Models
{
    // ENUM'I SINIFIN DIŞINA, NAMESPACE İÇİNE ALDIK (Her yerden kolayca erişmek için)
    public enum SiparisDurumu
    {
        Hazirlaniyor = 1,
        KargoyaVerildi = 2,
        TeslimEdildi = 3,
        IptalEdildi = 4
    }

    public class Siparis
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kullanıcı zorunludur")]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        [Required(ErrorMessage = "Sipariş tarihi zorunludur")]
        public DateTime SiparisTarihi { get; set; }

        [Required(ErrorMessage = "Ad zorunludur")]
        [StringLength(50, MinimumLength = 2)]
        public string Ad { get; set; }

        [Required(ErrorMessage = "Soyad zorunludur")]
        [StringLength(50, MinimumLength = 2)]
        public string Soyad { get; set; }

        [Required(ErrorMessage = "Telefon zorunludur")]
        [Phone(ErrorMessage = "Geçerli telefon numarası giriniz")]
        public string Telefon { get; set; }

        [Required(ErrorMessage = "Adres zorunludur")]
        [StringLength(200, MinimumLength = 10)]
        public string Adres { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Toplam tutar 0'dan büyük olmalıdır")]
        public decimal ToplamTutar { get; set; }

        public List<SiparisDetay> SiparisDetaylari { get; set; } = new List<SiparisDetay>();

        // 🔥 İŞTE EKSİK OLAN VE 22 HATAYI DOĞURAN KRİTİK ALAN:
        // Sipariş ilk oluşturulduğunda otomatik olarak "Hazirlaniyor" durumunu alır.
        public SiparisDurumu Durum { get; set; } = SiparisDurumu.Hazirlaniyor;
    }
}