using CajeroPOS.Models;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using static CajeroPOS.MainWindow;

namespace CajeroPOS.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Models.Producto> Productos { get; set; }
        public DbSet<Models.Usuario> Usuarios { get; set; }
        public DbSet<Categoria> Categorias { get; set; }

        public DbSet<Venta> Ventas { get; set; }
        public DbSet<VentasDetalles> VentaDetalles { get; set; }

        //Utiliza la conexion por defecto en el App.config
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string connectionString =
                    ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

                optionsBuilder.UseMySql(
                    connectionString,
                    ServerVersion.AutoDetect(connectionString)
                );
            }
        }
    }
}