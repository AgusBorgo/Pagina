namespace PaginaWeb.Models.ViewModel
{
    // Lo que ve el administrador al editar un usuario (sin contraseña)
    public class UsuarioAdminViewModel
    {
        public int UsuarioId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public int RolId { get; set; }
    }
}
