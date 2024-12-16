using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TravelApp.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Baðlantý dizesini ekliyoruz
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// DbContext'e baðlantýyý saðlýyoruz
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Identity servislerini ekliyoruz
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true; // Kullanýcý hesabý onaylamayý zorunlu tut
})
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Diðer servisler
builder.Services.AddControllersWithViews();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint(); // Migration iþlemleri için hata ayýklama sayfasýný etkinleþtir
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Ortak middleware yapýlandýrmasý
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Identity kimlik doðrulamasý
app.UseAuthorization();  // Yetkilendirme middleware'i

// Varsayýlan rota
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); // Razor sayfalarýný dahil eder

// Migration'larý otomatik uygulama (opsiyonel, development için önerilir)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();

    try
    {
        // Veritabanýný güncelle
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Migration error: {ex.Message}");
    }
}

app.Run();
