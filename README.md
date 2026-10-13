# BORGO Discos

Disquería online de CDs de rock nacional, hecha con **ASP.NET Core MVC (.NET 8)**, **Entity Framework Core** y **SQL Server**.

Empezó como trabajo práctico de la carrera de Analista en Sistemas (ORT) y la fui convirtiendo en un proyecto de portfolio: rediseño completo, seguridad en el login y un panel de administración.

> 📸 _Agregar capturas acá: inicio, catálogo, detalle de disco, carrito y panel de administración._

## Qué se puede hacer

**Como cliente**
- Recorrer el catálogo filtrando por género, buscar por título o artista y paginar resultados.
- Ver el detalle de cada disco con discos relacionados del mismo artista o género.
- Armar un carrito (guardado en una cookie, funciona sin iniciar sesión) que respeta el stock disponible.
- Crear una cuenta, iniciar sesión, confirmar la compra y ver el historial y el detalle de cada pedido.
- Editar sus datos personales.

**Como administrador**
- Panel con métricas: discos, faltantes de stock, pedidos, clientes y total vendido.
- Alta, edición y baja de discos con vista previa de la tapa. Si un disco ya se vendió, se oculta en lugar de borrarse para no romper el historial.
- Gestión de géneros (no permite borrar uno que todavía tiene discos).
- Consulta de pedidos y usuarios, y cambio de rol.

## Decisiones técnicas

- **Autenticación con cookies** y **autorización por roles** (`[Authorize(Policy = "AdminOnly")]`). Las páginas de cuenta están cerradas por defecto y solo se abren las públicas con `[AllowAnonymous]`.
- **Contraseñas hasheadas** con `PasswordHasher<T>` (incluye *salt*). El login muestra el mismo mensaje si falla el email o la contraseña, para no revelar qué emails existen.
- **ViewModels** para los formularios (registro, login, perfil): separan lo que llega del formulario de la entidad que se guarda y evitan el *overposting*. El rol siempre lo asigna el servidor.
- **Protección CSRF** con `[ValidateAntiForgeryToken]` en todas las acciones POST.
- **Seeding** al iniciar la app (`Data/DatosIniciales.cs`): crea los roles y, si no hay ninguno, un usuario administrador tomado de la configuración.
- **Servicios con interfaces** (`IProductoServices`, `ICategoriaServices`) inyectados por dependencia.
- La compra descuenta stock y crea el pedido en un único `SaveChanges`. El precio se guarda en cada línea del pedido para que el historial no cambie si después se modifica el precio.
- Diseño propio sin plantillas: CSS con variables (`wwwroot/css/site.css`), tipografías Anton, Archivo e IBM Plex Mono, responsive y con foco visible para navegar con teclado.

## Cómo correrlo

Requisitos: Visual Studio 2022 con la carga de trabajo **Desarrollo de ASP.NET y web** (incluye .NET 8 y SQL Server LocalDB).

1. Clonar el repo y abrir `PaginaWeb.sln`.
2. En *Herramientas → Administrador de paquetes NuGet → Consola del Administrador de paquetes*:
   ```
   Update-Database
   ```
3. Ejecutar con **F5**.
4. Entrar con el administrador inicial que está en `appsettings.Development.json` (`admin@borgo.com` / `Borgo2026!`), crear un género desde *Administración → Géneros* y cargar discos.

> El usuario administrador inicial solo se crea si la base todavía no tiene ningún administrador. En un entorno real, esos datos no deberían estar en el repositorio: se configuran con *User Secrets* o variables de entorno.

## Estructura

```
Controllers/   Tienda (Home, Carrito, Account) y administración (Dashboard, Productoes, Categorias, Pedidoes, Clientes)
Models/        Entidades de la base y ViewModels de los formularios
Services/      Consultas del catálogo
Data/          Datos iniciales (roles y administrador)
Helpers/       Formato de precios y fechas para Argentina
Views/         Vistas Razor
wwwroot/css/   Estilos del sitio
```

## Próximos pasos

- Publicarlo online con una base de datos en la nube.
- Subir las tapas como archivos en lugar de usar URLs.
- Estados de pedido (pendiente, enviado, entregado) gestionados desde el panel.
- Tests automatizados de los controladores.

---

Hecho por **Agustina Borgoña**.
