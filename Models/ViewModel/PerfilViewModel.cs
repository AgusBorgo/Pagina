using System.ComponentModel.DataAnnotations;

namespace PaginaWeb.Models.ViewModel
{
    // Datos que el usuario puede editar de su propio perfil
    public class PerfilViewModel
    {
        [Required(ErrorMessage = "Ingresá tu nombre.")]
        [StringLength(50, ErrorMessage = "El nombre no puede tener más de 50 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingresá tu apellido.")]
        [StringLength(50, ErrorMessage = "El apellido no puede tener más de 50 caracteres.")]
        public string Apellido { get; set; } = string.Empty;

        // Solo se muestra: el email no se puede cambiar desde el perfil
        public string? Email { get; set; }

        [Required(ErrorMessage = "Ingresá un teléfono.")]
        [Phone(ErrorMessage = "El teléfono no tiene un formato válido.")]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; } = string.Empty;

        [Display(Name = "Dirección")]
        public string? Direccion { get; set; }

        public int CantidadPedidos { get; set; }
    }
}
