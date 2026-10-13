using Microsoft.AspNetCore.Identity;
using PaginaWeb.Context;
using PaginaWeb.Models;

namespace PaginaWeb.Data
{
    // Datos que la app necesita sí o sí para funcionar ("seeding").
    // Se ejecuta al arrancar y no duplica nada si los datos ya existen.
    public static class DatosIniciales
    {
        public const string RolAdministrador = "Administrador";
        public const string RolCliente = "Cliente";

        public static void Cargar(PaginaDatabaseContext db, IConfiguration config, ILogger logger)
        {
            // 1. Roles
            foreach (var nombreRol in new[] { RolAdministrador, RolCliente })
            {
                if (!db.Roles.Any(r => r.Nombre == nombreRol))
                {
                    db.Roles.Add(new Rol { Nombre = nombreRol });
                }
            }
            db.SaveChanges();

            // 2. Usuario administrador (solo si todavía no hay ninguno)
            var rolAdmin = db.Roles.First(r => r.Nombre == RolAdministrador);
            if (db.Clientes.Any(c => c.RolId == rolAdmin.RolId))
            {
                return;
            }

            var email = config["AdminInicial:Email"];
            var contrasena = config["AdminInicial:Contrasena"];
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(contrasena))
            {
                logger.LogWarning("No hay administrador y falta la sección AdminInicial en la configuración.");
                return;
            }

            var admin = new Cliente
            {
                Nombre = "Admin",
                Apellido = "BORGO",
                Email = email,
                Telefono = "0000000000",
                RolId = rolAdmin.RolId
            };
            admin.Contrasena = new PasswordHasher<Cliente>().HashPassword(admin, contrasena);

            db.Clientes.Add(admin);
            db.SaveChanges();
            logger.LogInformation("Administrador inicial creado: {Email}", email);
        }
    }
}
