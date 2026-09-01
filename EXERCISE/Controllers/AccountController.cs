using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using EXERCISE_MVC.Models;
using EXERCISE_MVC.Data; // AppDbContext'e erişebilmek için ekledik

namespace EXERCISE_MVC.Controllers
{
    public class AccountController : Controller
    {
        // VERİTABANI BAĞLANTISI
        private readonly AppDbContext _context;

        // Constructor üzerinden veritabanı köprümüzü içeri alıyoruz
        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 1. E-posta adresi sistemde zaten kayıtlı mı kontrolü
            var varMiKullanici = _context.Users.Any(u => u.Email == model.Email);
            if (varMiKullanici)
            {
                ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanımda.");
                return View(model);
            }

            // 2. Yeni kullanıcı nesnesi oluşturma
            // DİKKAT: Çakışma yaşamamak için açıkça EXERCISE_MVC.Models.User olarak türetiyoruz
            var yeniKullanici = new EXERCISE_MVC.Models.User
            {
                Email = model.Email,
                Ad = model.Ad,       // RegisterViewModel içinde Ad alanının olduğunu varsayıyoruz
                Soyad = model.Soyad, // RegisterViewModel içinde Soyad alanının olduğunu varsayıyoruz

                // ŞİMDİLİK TEST AMAÇLI: Şifreyi açık metin kaydediyoruz. 
                // (İleride burayı BCrypt ile hash'leyeceğiz, Seed verilerinle tam uyum için şimdilik düz yazdık)
                SifreHash = model.Sifre,

                Role = UserRole.Customer, // Yeni kayıt olan herkes varsayılan olarak Müşteridir
                AktifMi = true,
                OlusturmaTarihi = DateTime.Now
            };

            // 3. Veritabanına ekleme ve kaydetme
            _context.Users.Add(yeniKullanici);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Kayıt işleminiz başarılı! Şimdi giriş yapabilirsiniz.";
            return RedirectToAction("Login", "Account");
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // VERİTABANINDAN KULLANICIYI BULMA
            // Çakışmayı önlemek için nesne tipini açıkça belirttik
            EXERCISE_MVC.Models.User kullanici = _context.Users
                .FirstOrDefault(u => u.Email == model.Email);

            // Kullanıcı bulunamadıysa veya şifre eşleşmiyorsa hata ver
            // (Seed verilerindeki hash'li şifreler için 'admin123' ve 'customer123' yerine şimdilik düz metin kontrolü yapıyoruz)
            if (kullanici == null || kullanici.SifreHash != model.Sifre)
            {
                ModelState.AddModelError("", "E-posta veya şifre hatalı!");
                return View(model);
            }

            // Kullanıcı pasif duruma getirilmişse içeri alma
            if (!kullanici.AktifMi)
            {
                ModelState.AddModelError("", "Hesabınız askıya alınmıştır. Lütfen yönetimle iletişime geçin.");
                return View(model);
            }

            // KİMLİK KARTINI HAZIRLAMA (Claims)
            // Veritabanındaki gerçek Ad, Soyad ve Rol bilgilerini karta basıyoruz
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, $"{kullanici.Ad} {kullanici.Soyad}"),
                new Claim(ClaimTypes.Email, kullanici.Email),
                new Claim(ClaimTypes.Role, kullanici.Role.ToString()) // Enum tipini string'e ("Admin" veya "Customer") çeviriyoruz
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // ÇEREZİ OLUŞTURMA VE OTURUMU AÇMA
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            // AKILLI YÖNLENDİRME
            if (kullanici.Role == UserRole.Admin)
            {
                return RedirectToAction("Index", "Admin");
            }

            return RedirectToAction("Index", "Home");
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}