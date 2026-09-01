
using EXERCISE_MVC.Models;
using EXERCISE_MVC.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.SqlServer;

namespace EXERCISE_MVC.Controllers
{
    public class HomeController : Controller
    {
        // 1. DEĞİŞİKLİK: Statik listeyi sildik, yerine AppDbContext (Veritabanı) ekledik.
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        // LİSTE SAYFASI (Index) - SQL Server üzerinden filtreleme
        // 1. Metodun başına "async" ve dönüş tipine "Task<IActionResult>" ekledik
        public async Task<IActionResult> Index(string search, string kategori)
        {
            // Veritabanındaki ürünleri sorgu olarak hazırlıyoruz
            var urunSorgusu = _context.Urunler.AsQueryable();

            // 1. Arama Filtresi (SQL tabanlı)
            if (!string.IsNullOrEmpty(search))
            {
                urunSorgusu = urunSorgusu.Where(x => x.Ad.Contains(search));
                ViewBag.SearchTerm = search;
            }

            // 2. Kategori Filtresi (SQL tabanlı)
            if (!string.IsNullOrEmpty(kategori))
            {
                urunSorgusu = urunSorgusu.Where(x => x.Kategori == kategori);
                ViewBag.SelectedKategori = kategori;
            }

            // 3. ToList() yerine "await ... ToListAsync()" kullanarak asenkron bitirdik
            var urunListesi = await urunSorgusu.ToListAsync();

            return View(urunListesi);
        }

        // KESİN ÇÖZÜM: Garson artık mutfak kapısında beklemiyor!
        public async Task<IActionResult> Detay(int id)
        {
            // 'await' koyarak veritabanından verinin gerçekten gelmesini asenkron bekliyoruz
            var bulunanUrun = await _context.Urunler.FirstOrDefaultAsync(x => x.Id == id);

            if (bulunanUrun == null)
            {
                return NotFound();
            }

            // Artık sayfaya 'Task' değil, gerçek 'Urun' nesnesi gidiyor!
            return View(bulunanUrun);
        }

        public async Task<IActionResult> SepeteEkle(int id)
        {
            // 1. Veritabanından eklenmek istenen ürünü bulalım
            var urun = await _context.Urunler.FirstOrDefaultAsync(u => u.Id == id);

            if (urun == null)
            {
                return NotFound(); // Ürün yoksa hata dön
            }

            // 2. Mevcut sepeti session'dan çekelim
            var sepetJson = HttpContext.Session.GetString("Sepetim");
            List<SepetItem> sepet = string.IsNullOrEmpty(sepetJson)
                ? new List<SepetItem>()
                : JsonSerializer.Deserialize<List<SepetItem>>(sepetJson);

            // 3. Bu ürün sepette zaten var mı diye bakalım
            var sepetItem = sepet.FirstOrDefault(s => s.Urun.Id == id);

            // Sepetteki mevcut adedi bulalım (Yoksa 0'dır)
            int sepettekiAdet = sepetItem != null ? sepetItem.Adet : 0;

            // KİRTIK KONTROL: Sepetteki adet + 1, SQL'deki stoktan büyük mü?
            if (sepettekiAdet + 1 > urun.Stok)
            {
                // Eğer stok yetersizse sepete EKLEME, direkt sepet sayfasına gönder
                // (İstersen buraya bir uyarı mesajı da ekleyebiliriz TempData ile)
                return RedirectToAction("Sepetim");
            }

            // 4. Eğer stok yetiyorsa ekleme işlemini yapalım
            if (sepetItem != null)
            {
                sepetItem.Adet++; // Zaten varsa adedi artır
            }
            else
            {
                sepet.Add(new SepetItem { Urun = urun, Adet = 1 }); // Yoksa yeni ekle
            }

            // 5. Sepetin güncel halini session'a geri mühürle
            HttpContext.Session.SetString("Sepetim", JsonSerializer.Serialize(sepet));

            return RedirectToAction("Sepetim");
        }

        public IActionResult Sepetim()
        {
            var sepetJson = HttpContext.Session.GetString("Sepetim");
            var sepet = new List<SepetItem>();

            if (!string.IsNullOrEmpty(sepetJson))
            {
                sepet = JsonSerializer.Deserialize<List<SepetItem>>(sepetJson);
            }

            return View(sepet);
        }

        public IActionResult SepetiTemizle()
        {
            HttpContext.Session.Remove("Sepetim");
            return RedirectToAction("Sepetim");
        }

        public async Task<IActionResult> SepettenSil(int id)
        {
            var sepetJson = HttpContext.Session.GetString("Sepetim");
            if (!string.IsNullOrEmpty(sepetJson))
            {
                var sepet = JsonSerializer.Deserialize<List<SepetItem>>(sepetJson);
                var silinecek = sepet.FirstOrDefault(x => x.Urun.Id == id);
                if (silinecek != null)
                {
                    sepet.Remove(silinecek);
                    HttpContext.Session.SetString("Sepetim", JsonSerializer.Serialize(sepet));
                }
            }
            return RedirectToAction("Sepetim");
        }

        public async Task<IActionResult> AdetAzalt(int id)
        {
            var sepetJson = HttpContext.Session.GetString("Sepetim");
            if (!string.IsNullOrEmpty(sepetJson))
            {
                var sepet = JsonSerializer.Deserialize<List<SepetItem>>(sepetJson);
                var item = sepet.FirstOrDefault(x => x.Urun.Id == id);
                if (item != null)
                {
                    item.Adet--;
                    if (item.Adet <= 0) sepet.Remove(item);
                    HttpContext.Session.SetString("Sepetim", JsonSerializer.Serialize(sepet));
                }
            }
            return RedirectToAction("Sepetim");
        }

        public async Task<IActionResult> AdetArtir(int id)
        {
            var sepetJson = HttpContext.Session.GetString("Sepetim");
            if (!string.IsNullOrEmpty(sepetJson))
            {
                var sepet = JsonSerializer.Deserialize<List<SepetItem>>(sepetJson);
                var item = sepet.FirstOrDefault(x => x.Urun.Id == id);

                if (item != null)
                {
                    // BURASI KRİTİK: Veritabanına asenkron olarak gidip o ürünün SQL'deki güncel satırını buluyoruz
                    var urunVeritabanindan = await _context.Urunler.FirstOrDefaultAsync(u => u.Id == id);

                    if (urunVeritabanindan != null)
                    {
                        // Sabit "10" yerine, SQL'den gelen urunVeritabanindan.Stok değerini kullanıyoruz
                        if (item.Adet < urunVeritabanindan.Stok)
                        {
                            item.Adet++;
                        }
                        else
                        {
                            // Stok sınırına dayandık!
                            TempData["Hata"] = "Stokta sadece " + urunVeritabanindan.Stok + " adet var.";
                        }
                    }

                    HttpContext.Session.SetString("Sepetim", JsonSerializer.Serialize(sepet));
                }
            }
            return RedirectToAction("Sepetim");
        }

        public IActionResult Odeme() => View();

        [HttpPost]
        public async Task<IActionResult> SiparisiTamamla(SiparisBilgileri bilgiler)
        {
            // 1. Önce sepeti (Session) çekelim (Silmeden önce okumamız lazım!)
            var sepetJson = HttpContext.Session.GetString("Sepetim");

            if (!string.IsNullOrEmpty(sepetJson))
            {
                // JSON formatındaki sepeti listeye çeviriyoruz
                var sepet = JsonSerializer.Deserialize<List<SepetItem>>(sepetJson);

                // YENİ ADIM: Yeni bir ana Sipariş kaydı oluşturuyoruz ve formdan gelen bilgilerle dolduruyoruz
                var yeniSiparis = new Siparis
                {
                    SiparisTarihi = DateTime.Now,
                    Ad = bilgiler.Ad,          // SiparisBilgileri modelinden gelen Ad
                    Soyad = bilgiler.Soyad,    // SiparisBilgileri modelinden gelen Soyad
                    Telefon = bilgiler.Telefon,// SiparisBilgileri modelinden gelen Telefon
                    Adres = bilgiler.Adres,    // SiparisBilgileri modelinden gelen Adres
                    ToplamTutar = sepet.Sum(s => s.Urun.Fiyat * s.Adet) // Genel Toplam Tutar
                };

                // 2. Sepetteki her ürün için SQL'e gidelim
                foreach (var item in sepet)
                {
                    // Veritabanından o ürünü ID'sine göre ASENKRON olarak buluyoruz (Timeout engellemek için)
                    var urun = await _context.Urunler.FirstOrDefaultAsync(u => u.Id == item.Urun.Id);

                    if (urun != null)
                    {
                        // SQL'deki mevcut stoktan, sepetteki adet kadar düşüyoruz
                        urun.Stok -= item.Adet;

                        // YENİ ADIM: Her ürün için bir Sipariş Detayı oluşturup ana siparişe bağlıyoruz
                        var detay = new SiparisDetay
                        {
                            UrunId = urun.Id,
                            Adet = item.Adet,
                            AnlikFiyat = urun.Fiyat // Ürünün o anki satış fiyatını mühürledik
                        };

                        // Detayı siparişin listesine ekle
                        // Eğer burada eski İngilizce isim kaldıysa hata verir, hemen değiştir:
                        yeniSiparis.SiparisDetaylari.Add(detay);
                    }
                }

                // YENİ ADIM: Hazırladığımız bu sipariş kartını DbContext listesine ekliyoruz
                _context.Siparisler.Add(yeniSiparis);

                // 3. BURASI EN ÖNEMLİSİ: Hem stok düşümlerini hem de sipariş kayıtlarını SQL'e tek seferde mühürle/kaydet
                yeniSiparis.UserId = 2;
                await _context.SaveChangesAsync();

                // 4. Stoklar düştükten sonra artık sepeti temizleyebiliriz
                HttpContext.Session.Remove("Sepetim");
            }

            // 5. Müşteriyi teşekkür sayfasına bilgilerle gönder (Senin orijinal yapın aynen duruyor)
            return View("Basarili", bilgiler);
        }
    }
}