using System.ComponentModel.DataAnnotations;

namespace EXERCISE_MVC.Models
{
    public class User
    {
        // 1. ID - Veritabanı Primary Key
        public int Id { get; set; }

        // 2. EMAIL - Kullanıcı emaili (login için kullanılır)
        [Required(ErrorMessage = "Email zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli email giriniz")]
        [StringLength(100)]  // SQL'de max 100 karakter
        public string Email { get; set; }

        // 3. ŞİFRE HASH - SADECE HASH'LENMIŞ ŞİFRE SAKLANIR!
        // Örnek: admin123 → $2a$11$KIXxPfxL7j1GGFHCVFnU9eP8z2K8...
        [Required(ErrorMessage = "Şifre zorunludur")]
        [StringLength(255)]  // Hash daha uzun olduğu için 255
        public string SifreHash { get; set; }

        // 4. AD - Kullanıcının adı
        [Required(ErrorMessage = "Ad zorunludur")]
        [StringLength(50, MinimumLength = 2)]
        public string Ad { get; set; }

        // 5. SOYAD - Kullanıcının soyadı
        [Required(ErrorMessage = "Soyad zorunludur")]
        [StringLength(50, MinimumLength = 2)]
        public string Soyad { get; set; }

        // 6. ROLE - Kullanıcının yetkisi (Admin mı yoksa Customer mı?)
        // Enum dediğimiz şey: Sadece belirli değerler alabilecek tip
        // Örneğin: UserRole.Admin veya UserRole.Customer
        [Required]
        public UserRole Role { get; set; }

        // 7. AKTİFMİ - Hesap aktif mi yoksa pasif mi?
        // Örneğin: Admin bir müşteriyi bloke etmek isterse AktifMi = false yapabilir
        public bool AktifMi { get; set; } = true;  // Varsayılan olarak aktif

        // 8. OLUŞTURMA TARİHİ - Hesap ne zaman açıldı?
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

        // 9. İLİŞKİ - Bu kullanıcının kaç siparişi var?
        // Bir kullanıcı birden fazla sipariş verebilir
        // Siparişler tablosundaki UserId buna referans verir
        public List<Siparis> Siparisler { get; set; } = new List<Siparis>();
    }

    // ROLE ENUM
    // Sadece 2 seçenek: Admin veya Customer
    // Bu sayede yanlış bir role ataması yapılamaz
    public enum UserRole
    {
        Admin = 1,      // Yönetici
        Customer = 2    // Müşteri
    }
}