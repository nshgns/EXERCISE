// ============================================
// USING STATEMENTS - Gerekli kütüphaneler
// ============================================

using EXERCISE_MVC.Models;
using Microsoft.EntityFrameworkCore;

namespace EXERCISE_MVC.Data
{
    // ============================================
    // APP DB CONTEXT - Veritabanı Bağlantısı
    // ============================================
    // Bu sınıf, uygulama ile SQL Server arasında köprü
    // DbSet = Veritabanı tablosu
    public class AppDbContext : DbContext
    {
        // ============================================
        // CONSTRUCTOR (KURUCU)
        // ============================================
        // Dependency Injection ile DbContextOptions alır
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // ============================================
        // DB SETLER (VERİTABANI TABLOLARI)
        // ============================================
        // Her DbSet = Bir tablo
        // Örnek: Urunler = Urunler tablosu

        // Ürünler tablosu
        public DbSet<Urun> Urunler { get; set; }

        // Siparişler tablosu
        public DbSet<Siparis> Siparisler { get; set; }

        // Sipariş detayları tablosu
        public DbSet<SiparisDetay> SiparisDetaylari { get; set; }

        // YENİ: Kullanıcılar tablosu (Admin + Customer)
        public DbSet<User> Users { get; set; }

        // ============================================
        // MODELBUILDER CONFIGURATION (MODEL AYARLARI)
        // ============================================
        // Bu metod, veritabanı şemasını özelleştirir
        // Foreign Key, veri tipi, başlangıç verileri vb.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ============================================
            // TABLO İSİMLERİNİ SABİTLE
            // ============================================
            // EF Core'un "-s" takısı eklemesini engelle
            // Örnek: Sipariş → Siparisler (Türkçe'de -ler takısı)

            // Siparisler tablosunun adı sabit olsun
            modelBuilder.Entity<Siparis>().ToTable("Siparisler");

            // SiparisDetaylari tablosunun adı sabit olsun
            modelBuilder.Entity<SiparisDetay>().ToTable("SiparisDetaylari");

            // ============================================
            // PROPERTY (ALAN) KONFIGÜRASYONU
            // ============================================

            // 1. Fiyat alanının SQL tipi = decimal(18,2)
            // decimal(18,2) = Tam sayı 18 basamak, ondalık 2 basamak
            // Örnek: 9.999.999.999.999.999,99
            modelBuilder.Entity<Urun>()
                .Property(u => u.Fiyat)
                .HasColumnType("decimal(18,2)");

            // ============================================
            // FOREIGN KEY İLİŞKİLERİ
            // ============================================

            // İlişki 1: SiparisDetay → Siparis
            // "Her sipariş detayının bir siparişi vardır"
            // "Bir siparişin birden fazla detayı olabilir"
            modelBuilder.Entity<SiparisDetay>()
                .HasOne(sd => sd.Siparis)                    // SiparisDetay'ın BİR Sipariş'i var
                .WithMany(s => s.SiparisDetaylari)           // Sipariş'in ÇOK detayı olabilir
                .HasForeignKey(sd => sd.SiparisId)           // Foreign Key = SiparisId
                .OnDelete(DeleteBehavior.Cascade);           // Sipariş silinirse, detayları da silinir

            // İlişki 2: SiparisDetay → Urun
            // "Her sipariş detayının bir ürünü vardır"
            // "Bir ürün birden fazla detayda olabilir"
            modelBuilder.Entity<SiparisDetay>()
                .HasOne(sd => sd.Urun)                       // SiparisDetay'ın BİR Ürün'ü var
                .WithMany()                                  // Ürün'ün ÇOK detayı olabilir
                .HasForeignKey(sd => sd.UrunId)              // Foreign Key = UrunId
                .OnDelete(DeleteBehavior.Restrict);          // Ürün silinirse, detaylar KALIR (Tarihi veri)

            // İlişki 3: YENİ - Siparis → User
            // "Her siparişin bir kullanıcısı vardır"
            // "Bir kullanıcının birden fazla siparişi olabilir"
            modelBuilder.Entity<Siparis>()
                .HasOne(s => s.User)                         // Sipariş'in BİR User'ı var
                .WithMany(u => u.Siparisler)                 // User'ın ÇOK sipariş'i olabilir
                .HasForeignKey(s => s.UserId)                // Foreign Key = UserId
                .OnDelete(DeleteBehavior.Cascade);           // Kullanıcı silinirse, siparişleri de silinir

            // ============================================
            // BAŞLANGIÇ VERİLERİ (SEED DATA)
            // ============================================
            // Migration oluşturulduğunda, bu veriler otomatik eklenir

            // Başlangıç Ürünleri
            modelBuilder.Entity<Urun>().HasData(
                new Urun
                {
                    Id = 1,
                    Ad = "Akıllı Saat",
                    Fiyat = 3500.50m,
                    Kategori = "Elektronik",
                    ResimYolu = "saat.jpg",
                    Stok = 10
                },
                new Urun
                {
                    Id = 2,
                    Ad = "Kablosuz Kulaklık",
                    Fiyat = 1200.00m,
                    Kategori = "Elektronik",
                    ResimYolu = "Kulaklık.jpg",
                    Stok = 10
                },
                new Urun
                {
                    Id = 3,
                    Ad = "Bluetooth Hoparlör",
                    Fiyat = 850.75m,
                    Kategori = "Aksesuar",
                    ResimYolu = "Hoparlör.jpg",
                    Stok = 10
                }
            );

            // YENİ: Başlangıç Kullanıcıları
            // Bu kullanıcılar, migration'da otomatik oluşturulur
            // Test amaçlı login yapabilirsiniz
            // ============================================
            // YENİ: Başlangıç Kullanıcıları
            // ============================================
            // Başına projenin tam adını ekleyerek (EXERCISE_MVC.Models.User) çakışmayı çözüyoruz
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Email = "admin@example.com",
                    // Şifre: admin123 (BCrypt hash'lenmiş)
                    SifreHash = "$2a$11$KIXxPfxL7j1GGFHCVFnU9eP8z2K8Ynzq7vQZqYzQ2Y4Y2k6Y2oC2",
                    Ad = "Admin",
                    Soyad = "Yönetici",
                    Role = UserRole.Admin,
                    AktifMi = true,
                    OlusturmaTarihi =new DateTime(2026, 1, 1)
                },
                new User
                {
                    Id = 2,
                    Email = "customer@example.com",
                    // Şifre: customer123 (BCrypt hash'lenmiş)
                    SifreHash = "$2a$11$KIXxPfxL7j1GGFHCVFnU9eP8z2K8Ynzq7vQZqYzQ2Y4Y2k6Y2oC2",
                    Ad = "Ali",
                    Soyad = "Müşteri",
                    Role = UserRole.Customer,
                    AktifMi = true,
                    OlusturmaTarihi = new DateTime(2026, 1, 1)
                }
            );
        }
    }
}