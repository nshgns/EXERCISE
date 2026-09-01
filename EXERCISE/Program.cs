// ============================================
// USING STATEMENTS - Gerekli kütüphaneler
// ============================================

using EXERCISE_MVC.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

// Not: Microsoft.AspNetCore.Authentication.Cookies nedir?
// Cookie ile kullanıcı oturumunu saklamak için gerekli
// Örneğin: "Ali giriş yaptı" bilgisini cookie'de tutarız

// ============================================
// BUILDER OLUŞTUR
// ============================================

var builder = WebApplication.CreateBuilder(args);

// --- SERVİS KAYITLARI (builder.Build'den ÖNCE) ---
// "Servis" = Uygulama başladığında kullanılacak bileşenler

// 1. CONTROLLERS VE VIEWS
// MVC mimarisini etkinleştir (Controller + View desteği)
builder.Services.AddControllersWithViews();

// 2. DATABASE BAĞLANTISI
// AppDbContext'i DI (Dependency Injection) konteynerine ekle
// Böylece Controller'larda _context = new AppDbContext(...) yazmanız gerekmez
// Otomatik olarak injected olur
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        }));// ============================================
// YENİ: AUTHENTICATION (KİMLİK DOĞRULAMA)
// ============================================

// 3. COOKIE AUTHENTICATION EKLE
// Bunu eklemezsek, [Authorize] attribute'ü çalışmaz!
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // Login sayfası - Giriş yapmayan kullanıcılar buraya yönlendirilir
        // Örnek: "/Account/Login" sayfasına git, giriş yap
        options.LoginPath = "/Account/Login";

        // Logout sayfası - Çıkış yap dediğinde nereye gitsin?
        // Örnek: "/Account/Logout" işlemini çalıştır, sonra ana sayfaya git
        options.LogoutPath = "/Account/Logout";

        // Access Denied sayfası - Yetkileri olmayan sayfaya girerse
        // Örnek: Müşteri Admin paneline girmeye çalışırsa → Bu sayfayı göster
        options.AccessDeniedPath = "/Account/AccessDenied";

        // Cookie ayarları
        options.Cookie.HttpOnly = true;     // JavaScript'ten erişilemesin (Güvenlik)
        options.Cookie.IsEssential = true;  // GDPR uyumlu
        options.ExpireTimeSpan = TimeSpan.FromDays(7); // 7 gün geçerli
    });

// ============================================
// SESSION (OTURUM) AYARLARI
// ============================================

// 4. SESSION EKLE
// Session = Müşterinin sepeti, kişisel bilgileri vb. saklamak için
builder.Services.AddSession(options =>
{
    // Session'ı ne kadar süre sakla?
    // 30 dakika kullanmadıktan sonra session sonlanır
    options.IdleTimeout = TimeSpan.FromMinutes(30);

    // HttpOnly = JavaScript'ten erişilemesin (Güvenlik)
    options.Cookie.HttpOnly = true;

    // IsEssential = Bu cookie olmadan site çalışamaz
    options.Cookie.IsEssential = true;
});

// ============================================
// DİĞER SERVİSLER
// ============================================

// 5. HTTP CONTEXT ACCESSOR
// Controller'da HttpContext.Current yerine HttpContextAccessor kullan
// Örneğin: User.FindFirst(ClaimTypes.NameIdentifier) - Giriş yapan kullanıcının ID'sini al
builder.Services.AddHttpContextAccessor();

// ============================================
// APP OLUŞTUR
// ============================================

// Build() = Tüm servisleri yükle ve app oluştur
// Bu satırdan sonra app kullanılır
var app = builder.Build();

// ============================================
// MIDDLEWARE AYARLARI (builder.Build'den SONRA)
// ============================================
// Middleware = İsteğin işlenmesi sırasında yapılacak işlemler (sıra önemli!)

// --- EXCEPTION HANDLING ---

// 6. DEVELOPMENT ORTAMINDA HATA DETAYLARI GÖR
// localhost'ta development ise, detaylı hata mesajlarını göster
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    // PRODUCTION ortamında ise, genel hata sayfası göster (Hacker'a detay verme!)
    app.UseExceptionHandler("/Home/Error");

    // HSTS = HTTP Strict-Transport-Security
    // "Hep HTTPS kullan" demek (Güvenlik)
    app.UseHsts();
}

// ============================================
// HTTPS VE STATIC FILES
// ============================================

// 7. HTTPS KULLAN
// HTTP isteklerini HTTPS'ye yönlendir (Güvenlik)
app.UseHttpsRedirection();

// 8. STATIC FILES
// wwwroot klasöründeki dosyaları (CSS, JS, resim) sunabilir
app.UseStaticFiles();

// ============================================
// YENİ: AUTHENTICATION MIDDLEWARE
// ============================================

// 9. ROUTING
// URL'den Controller/Action bulma işlemi
app.UseRouting();

// 10. AUTHENTICATION MIDDLEWARE (ÖNEMLİ: Session'dan SONRA, Authorization'dan ÖNCE!)
// Bu middleware, gelen isteğin kimliğini kontrol eder
// Cookie'den kullanıcı bilgisini çıkarır
app.UseAuthentication();

// 11. AUTHORIZATION MIDDLEWARE
// Bu middleware, kullanıcının erişebileceği sayfaları kontrol eder
// Örneğin: [Authorize(Roles = "Admin")] attribute'ü burada kontrol edilir
app.UseAuthorization();

// ============================================
// SESSION MIDDLEWARE
// ============================================

// 12. SESSION BAŞLAT
// Kullanıcı için session oluştur (sepet bilgileri vb. sakla)
app.UseSession();

// ============================================
// ROUTE AYARLARI
// ============================================

// 13. DEFAULT ROUTE
// Bir URL girildiğinde hangi Controller/Action'a gitsin?
// Örnek: http://localhost:5000/Home/Index
//        → HomeController.Index() metoduna git
//
// Örnek: http://localhost:5000/ (boş)
//        → HomeController.Index() metoduna git (default)
//
// Örnek: http://localhost:5000/Admin/Urunler
//        → AdminController.Urunler() metoduna git
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
// {controller=Home} = Controller belirtilmezse Home'a git
// {action=Index} = Action belirtilmezse Index'e git
// {id?} = id parametresi opsiyonel (? = optional)


// ============================================
// VERİTABANI OLUŞTUR VE MIGRATION UYGULA
// ============================================

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();

    try
    {
        // Veritabanını oluştur (eğer yoksa)
        // Tüm tabloları ve ilişkileri otomatik oluşturur
        context.Database.EnsureCreated();

        Console.WriteLine("✅ Veritabanı başarıyla oluşturuldu!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Veritabanı oluşturulurken hata: {ex.Message}");
    }
}

// ============================================
app.Run();
// ============================================
app.Run();
// ============================================
// UYGULAMAYI BAŞLAT
// ============================================

// 14. RUN = Uygulamayı başlat ve dinle
app.Run();