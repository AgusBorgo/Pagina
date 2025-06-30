using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaginaWeb.Models
{
    public class Producto
    {   

        [Key]
        public int ProductoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public string ImagenUrl { get; set; } = string.Empty;
        public int Stock { get; set; }
        public int CategoriaId { get; set; }
        [ForeignKey("CategoriaId")]
        public Categoria Categoria { get; set; } = new Categoria(); // Relación con la entidad Categoria
        public bool Activo { get; set; } = true; // Indica si el producto está activo o no
        public ICollection<PedidoDetalle> PedidoDetalles { get; set; } = new List<PedidoDetalle>(); // Relación con la entidad PedidoDetalle
    }
}
