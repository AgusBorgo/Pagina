using System.ComponentModel.DataAnnotations;

namespace PaginaWeb.Models
{
    public class Categoria
    {
        [Key]
        public int CategoriaId { get; set; }

        [Required(ErrorMessage = "Ingresá el nombre del género.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingresá una descripción.")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = string.Empty;

        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}
