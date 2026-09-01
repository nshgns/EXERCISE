using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EXERCISE_MVC.Models;
using EXERCISE_MVC.Data;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EXERCISE_MVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        // 1. VERİTABANI BAĞLANTISI (Dependency Injection)
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // ADIM 1: ÜRÜN LİSTELEME SİSTEMİ (READ)
        // ==========================================
        // GET: /Admin/Index
        // GET: /Admin/Index
        // GET: /Admin/Index
        public async Task<IActionResult> Index()
        {
            // 1. Ürünler tablosundaki tüm listeyi çekiyoruz
            var urunler = await _context.Urunler.ToListAsync();

            // 2. Siparişler tablosundaki toplam sipariş sayısını alıyoruz
            ViewBag.ToplamSiparisSayisi = await _context.Siparisler.CountAsync();

            // 3. NOKTA ATIŞI FİLTRE: Users tablosuna git, sadece Role hücresi 2 (Müşteri) olanları say!
            // Böylece rolü 1 olan Adminler bu sayıya dahil edilmez, istatistiğin şaşmaz.
            ViewBag.ToplamMusteriSayisi = await _context.Users.CountAsync(u => u.Role == UserRole.Customer);

            // Verileri HTML sayfasına fırlatıyoruz
            return View(urunler);
        }
        // ==========================================
        // ADIM 2: YENİ ÜRÜN EKLEME SAYFASI (CREATE - GET)
        // ==========================================
        // GET: /Admin/Create
        public IActionResult Create()
        {
            return View();
        }

        // ==========================================
        // ADIM 3: FORMDAN GELEN VERİYİ SQL'E KAYDETME (CREATE - POST)
        // ==========================================
        // POST: /Admin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Urun urun, IFormFile? resimDosyasi)
        {
            if (ModelState.IsValid)
            {
                if (resimDosyasi != null && resimDosyasi.Length > 0)
                {
                    var benzersizIsim = Guid.NewGuid().ToString() + "_" + Path.GetFileName(resimDosyasi.FileName);
                    var klasorYolu = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img");
                    var tamDosyaYolu = Path.Combine(klasorYolu, benzersizIsim);

                    if (!Directory.Exists(klasorYolu))
                    {
                        Directory.CreateDirectory(klasorYolu);
                    }

                    using (var stream = new FileStream(tamDosyaYolu, FileMode.Create))
                    {
                        await resimDosyasi.CopyToAsync(stream);
                    }

                    urun.ResimYolu = benzersizIsim;
                }

                _context.Urunler.Add(urun);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(urun);
        }
        

        // ==========================================
        // ADIM 4: ÜRÜN SİLME SİSTEMİ (DELETE)
        // ==========================================
        public async Task<IActionResult> Delete(int id)
        {
            var urun = await _context.Urunler.FindAsync(id);
            if (urun == null)
            {
                return RedirectToAction(nameof(Index));
            }

            if (!string.IsNullOrEmpty(urun.ResimYolu))
            {
                var dosyaYolu = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", urun.ResimYolu);
                if (System.IO.File.Exists(dosyaYolu))
                {
                    System.IO.File.Delete(dosyaYolu);
                }
            }

            _context.Urunler.Remove(urun);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // ADIM 5: ÜRÜN DÜZENLEME FORMU (EDIT - GET)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var urun = await _context.Urunler.FindAsync(id);
            if (urun == null)
            {
                return NotFound();
            }
            return View(urun);
        }

        // ==========================================
        // ADIM 6: GÜNCELLENEN VERİLERİ SQL'E YAZMA (EDIT - POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Urun guncelUrun, IFormFile? yeniResimDosyasi)
        {
            if (id != guncelUrun.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (yeniResimDosyasi != null && yeniResimDosyasi.Length > 0)
                    {
                        var eskiUrun = await _context.Urunler.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                        if (eskiUrun != null && !string.IsNullOrEmpty(eskiUrun.ResimYolu))
                        {
                            var eskiDosyaYolu = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", eskiUrun.ResimYolu);
                            if (System.IO.File.Exists(eskiDosyaYolu))
                            {
                                System.IO.File.Delete(eskiDosyaYolu);
                            }
                        }

                        var benzersizIsim = Guid.NewGuid().ToString() + "_" + Path.GetFileName(yeniResimDosyasi.FileName);
                        var klasorYolu = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img");
                        var tamDosyaYolu = Path.Combine(klasorYolu, benzersizIsim);

                        using (var stream = new FileStream(tamDosyaYolu, FileMode.Create))
                        {
                            await yeniResimDosyasi.CopyToAsync(stream);
                        }

                        guncelUrun.ResimYolu = benzersizIsim;
                    }
                    else
                    {
                        var mevcutUrun = await _context.Urunler.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                        if (mevcutUrun != null)
                        {
                            guncelUrun.ResimYolu = mevcutUrun.ResimYolu;
                        }
                    }

                    _context.Urunler.Update(guncelUrun);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Urunler.Any(e => e.Id == guncelUrun.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(guncelUrun);
        }

        // ==========================================
        // ADIM 7: SİPARİŞ KONTROL MERKEZİ (LİSTELEME)
        // ==========================================
        // GET: /Admin/Siparisler
        public async Task<IActionResult> Siparisler()
        {
            var siparisler = await _context.Siparisler
                .OrderByDescending(s => s.SiparisTarihi)
                .ToListAsync();

            return View(siparisler);
        }

        // ==========================================
        // ADIM 8: SİPARİŞ DETAY SAYFASI
        // ==========================================
        // GET: /Admin/SiparisDetay/5
        public async Task<IActionResult> SiparisDetay(int id)
        {
            var siparis = await _context.Siparisler
                .Include(s => s.SiparisDetaylari)
                .ThenInclude(d => d.Urun)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (siparis == null)
            {
                return NotFound();
            }

            return View(siparis);
        }

        // ==========================================
        // ADIM 9: SİPARİŞİ İPTAL ETME VE STOĞU GERİ YÜKLEME
        // ==========================================
        public async Task<IActionResult> SiparisIptal(int id)
        {
            var siparis = await _context.Siparisler
                .Include(s => s.SiparisDetaylari)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (siparis == null) return NotFound();

            // Enum kontrolü yapılıyor
            if (siparis.Durum != SiparisDurumu.IptalEdildi)
            {
                foreach (var detay in siparis.SiparisDetaylari)
                {
                    var urun = await _context.Urunler.FindAsync(detay.UrunId);
                    if (urun != null)
                    {
                        urun.Stok += detay.Adet;
                    }
                }

                siparis.Durum = SiparisDurumu.IptalEdildi;
                await _context.SaveChangesAsync();
                TempData["Mesaj"] = "Sipariş başarıyla iptal edildi ve ürün stokları iade edildi.";
            }

            return RedirectToAction(nameof(Siparisler));
        }

        // ==========================================
        // ADIM 10: SİPARİŞİ KARGOYA VERİLDİ OLARAK GÜNCELLEME
        // ==========================================
        public async Task<IActionResult> SiparisKargola(int id)
        {
            var siparis = await _context.Siparisler.FindAsync(id);
            if (siparis == null) return NotFound();

            siparis.Durum = SiparisDurumu.KargoyaVerildi;
            await _context.SaveChangesAsync();

            TempData["Mesaj"] = "Sipariş durumu 'Kargoya Verildi' olarak güncellendi.";
            return RedirectToAction(nameof(SiparisDetay), new { id = id });
        }

        // ==========================================
        // ADIM 11: SİPARİŞİ TESLİM EDİLDİ OLARAK GÜNCELLEME
        // ==========================================
        public async Task<IActionResult> SiparisTeslimEt(int id)
        {
            var siparis = await _context.Siparisler.FindAsync(id);
            if (siparis == null) return NotFound();

            siparis.Durum = SiparisDurumu.TeslimEdildi;
            await _context.SaveChangesAsync();

            TempData["Mesaj"] = "Sipariş durumu 'Teslim Edildi' olarak güncellendi.";
            return RedirectToAction(nameof(SiparisDetay), new { id = id });
        }
    } // AdminController Sınıf Kapanışı
} // Namespace Kapanışı