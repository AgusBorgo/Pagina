using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaginaWeb.Models
{
    public class Pedido
    {
        [Key]
        public int PedidoId { get; set; }
        [Required]
        public int UsuarioId { get; set; } // Relación con Cliente  
        [ForeignKey("UsuarioId")]
        public Cliente Usuario { get; set; } = new Cliente(); // Relación con la entidad Usuario  
        public DateTime FechaPedido { get; set; } = DateTime.Now;
        [Required]
        public string Estado { get; set; } = "Pendiente"; // Estado del pedido (Pendiente, Enviado, Entregado, Cancelado)  
        public decimal Total { get; set; } = 0.0m; // Total del pedido  

        // Fix for CS0053: Ensure PedidoDetalle is public to match the accessibility of PedidoDetalles  
        // Fix for IDE0028: Simplify collection initialization  
        public ICollection<PedidoDetalle> PedidoDetalles { get; set; } = new List<PedidoDetalle>(); // Relación con la entidad PedidoDetalle  
    }
}
