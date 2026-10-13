using System.ComponentModel.DataAnnotations;

namespace PaginaWeb.Models.ViewModel
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Ingresá tu email.")]
        [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingresá tu contraseña.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Contrasena { get; set; } = string.Empty;

        // Página a la que volver después de iniciar sesión (por ejemplo, el carrito)
        public string? ReturnUrl { get; set; }
    }
}
