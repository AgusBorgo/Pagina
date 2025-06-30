using System.ComponentModel.DataAnnotations;

namespace PaginaWeb.Models
{
    public class Rol
    {
        [Key]
        public int RolId { get; set; }
        [Required]
        [StringLength(50, ErrorMessage = "El nombre del rol no puede exceder los 50 caracteres.")]
        public string Nombre { get; set; } = string.Empty;
    }
}
