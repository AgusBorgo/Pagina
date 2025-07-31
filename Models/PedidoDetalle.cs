using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaginaWeb.Models
{
    public class PedidoDetalle
    {
        [Key]
        public int DetallePedidoId{ get; set; } 
        public int PedidoId { get; set; } // Relación con la entidad Pedido
        [ForeignKey("PedidoId")]
        public Pedido Pedido { get; set; } = null!; // Relación con la entidad Pedido
        public int ProductoId { get; set; } // Relación con la entidad Producto
        [ForeignKey("ProductoId")]
        [Required]
        public Producto Producto { get; set; } // Relación con la entidad Producto
        public int Cantidad { get; set; } // Cantidad del producto en el pedido
        public decimal PrecioUnitario { get; set; } // Precio unitario del producto al momento del pedido
        public decimal Subtotal => Cantidad * PrecioUnitario; // Subtotal del detalle del pedido
    }
}