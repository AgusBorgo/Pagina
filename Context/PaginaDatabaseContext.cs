using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PaginaWeb.Models;
using System.Collections.Generic;

namespace PaginaWeb.Context
{
    public class PaginaDatabaseContext : DbContext
    {
        public PaginaDatabaseContext(DbContextOptions<PaginaDatabaseContext> options) : base(options)
        {
        }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<PaginaWeb.Models.Producto> Producto { get; set; } = default!;
        public DbSet<Pedido> Pedidos { get; set; } = default!;
        public DbSet<PedidoDetalle> PedidoDetalles { get; set; } = default!;
        public DbSet<Rol> Roles { get; set; } = default!; // Agregar DbSet para Rol
        public DbSet<PaginaWeb.Models.Categoria> Categoria { get; set; } = default!;

    }
    
    
}
