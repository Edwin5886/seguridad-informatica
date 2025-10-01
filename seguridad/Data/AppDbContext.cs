using Microsoft.EntityFrameworkCore;
using seguridad.Models;

namespace seguridad.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<UsuarioSeguridad> Usuarios_Seguridad { get; set; }
        public DbSet<Operacion> Operaciones { get; set; }
        public DbSet<TablaAuditable> TablasAuditables { get; set; }
        public DbSet<BitacoraEncabezado> BitacoraEncabezado { get; set; }
        public DbSet<BitacoraDetalle> BitacoraDetalle { get; set; }
        // ... tus otras tablas
    }
}