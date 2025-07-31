using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using PaginaWeb.Context;
using PaginaWeb.Services;

namespace PaginaWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<PaginaDatabaseContext>(
 options =>
options.UseSqlServer(builder.Configuration["ConnectionString:PaginaDBConnection"]));

            // Permite tener un usuario administrador
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy =>
                    policy.RequireRole("Admin"));
            });

            // Configura la autenticación con cookies
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
            {
                options.LoginPath = "/Account/Login"; // Ruta de inicio de sesión
                options.AccessDeniedPath = "/Account/AccessDenied"; // Ruta de acceso denegado
                options.Cookie.HttpOnly = true; // Hace que la cookie no sea accesible desde JavaScript               
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Tiempo de expiración de la sesión
                options.SlidingExpiration = true; // Permite que la sesión se extienda si el usuario está activo
                options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always; // Asegura que la cookie solo se envíe a través de HTTPS
                options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict; // Previene el envío de cookies en solicitudes de sitios cruzados
            });

            builder.Services.AddScoped<IProductoServices, ProductoService>();
            builder.Services.AddScoped<ICategoriaServices, CategoriaService>();

            // Add services to the container.   
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication(); // Habilita la autenticación
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }

    }
}
