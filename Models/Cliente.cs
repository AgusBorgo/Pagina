using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace PaginaWeb.Models
{
    public class Cliente
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UsuarioId { get; set; }
        [Required]
        [StringLength(10, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;
        [Required]
        public string Apellido { get; set; } = string.Empty;
        [Required]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
        public string Email { get; set; } = string.Empty;
        [Required]
        [StringLength(20, ErrorMessage = "La contraseña no puede exceder los 20 caracteres.")]
        public string Contrasena { get; set; } = string.Empty;
        [Required]
        [Phone(ErrorMessage = "El formato del teléfono no es válido.")]
        public string Telefono { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public int RolId { get; set; } = 2; // Rol de cliente por defecto
        [ForeignKey("RolId")]
        public Rol Rol { get; set; } = new Rol(); // Relación con la entidad Rol
        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>(); // Relación con la entidad Pedido
        

    }
}
