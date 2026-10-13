using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaginaWeb.Models
{
    public class Producto
    {
        [Key]
        public int ProductoId { get; set; }

        [Required(ErrorMessage = "Ingresá el título del disco.")]
        [StringLength(150)]
        [Display(Name = "Título")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingresá el artista.")]
        [StringLength(150)]
        public string Artista { get; set; } = string.Empty;

        [Range(1900, 2100, ErrorMessage = "Ingresá un año válido.")]
        [Display(Name = "Año")]
        public int? Anio { get; set; }

        [Required(ErrorMessage = "Ingresá una descripción.")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = string.Empty;

        [Range(0, 99999999, ErrorMessage = "El precio no puede ser negativo.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Precio { get; set; }

        [Required(ErrorMessage = "Ingresá la URL de la imagen de tapa.")]
        [Display(Name = "URL de la tapa")]
        public string ImagenUrl { get; set; } = string.Empty;

        [Range(0, 100000, ErrorMessage = "El stock no puede ser negativo.")]
        public int Stock { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Elegí un género.")]
        [Display(Name = "Género")]
        public int CategoriaId { get; set; }

        [ForeignKey("CategoriaId")]
        public Categoria? Categoria { get; set; }

        [Display(Name = "Visible en la tienda")]
        public bool Activo { get; set; } = true;

        public ICollection<PedidoDetalle> PedidoDetalles { get; set; } = new List<PedidoDetalle>();
    }
}
