using System.Globalization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using PaginaWeb.Context;
using PaginaWeb.Data;
using PaginaWeb.Services;

namespace PaginaWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Base de datos
            builder.Services.AddDbContext<PaginaDatabaseContext>(options =>
                options.UseSqlServer(builder.Configuration["ConnectionString:PaginaDBConnection"]));

            // Autorización: la política AdminOnly exige el rol Administrador
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole(DatosIniciales.RolAdministrador));
            });

            // Autenticación con cookies
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.Cookie.HttpOnly = true;                       // JavaScript no puede leer la cookie
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                options.SlidingExpiration = true;                     // se renueva si el usuario sigue activo
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // solo por HTTPS
                options.Cookie.SameSite = SameSiteMode.Lax;
            });

            builder.Services.AddScoped<IProductoServices, ProductoService>();
            builder.Services.AddScoped<ICategoriaServices, CategoriaService>();

            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Carga roles y administrador inicial
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<PaginaDatabaseContext>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                DatosIniciales.Cargar(db, app.Configuration, logger);
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            // Los números de los formularios se leen siempre con punto decimal (1800.50),
            // sin importar el idioma de Windows. Los precios se muestran en pesos con Helpers/Formato.
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture(CultureInfo.InvariantCulture),
                SupportedCultures = new[] { CultureInfo.InvariantCulture },
                SupportedUICultures = new[] { CultureInfo.InvariantCulture }
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
