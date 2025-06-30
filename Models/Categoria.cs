using System.ComponentModel.DataAnnotations;

namespace PaginaWeb.Models
{
    public class Categoria
    {
        [Key]
        public int CategoriaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        
        public ICollection<Producto> Productos { get; set; } = new List<Producto>(); // Relación con la entidad Producto
   
    }
}