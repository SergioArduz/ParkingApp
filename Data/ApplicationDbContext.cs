using Microsoft.EntityFrameworkCore;
using ParkingApp.Models;
using Npgsql;

namespace ParkingApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }

        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<ParkingSession> ParkingSessions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Vehicle>()
                .HasIndex(v => v.Plate)
                .IsUnique();

            modelBuilder.Entity<ParkingSession>()
                .HasOne(ps => ps.Vehicle)
                .WithMany(v => v.ParkingSessions)
                .HasForeignKey(ps => ps.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        // Método para configurar la conexión dinámicamente
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Toma la variable de entorno DATABASE_URL
                var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

                if (!string.IsNullOrEmpty(databaseUrl))
                {
                    // Convierte DATABASE_URL de Render al formato Npgsql
                    var databaseUri = new Uri(databaseUrl);
                    var userInfo = databaseUri.UserInfo.Split(':');

                    var builder = new NpgsqlConnectionStringBuilder
                    {
                        Host = databaseUri.Host,
                        Port = databaseUri.Port,
                        Username = userInfo[0],
                        Password = userInfo[1],
                        Database = databaseUri.LocalPath.TrimStart('/'),
                        SslMode = SslMode.Require,
                        TrustServerCertificate = true
                    };

                    optionsBuilder.UseNpgsql(builder.ToString());
                }
                else
                {
                    // Conexión local si no hay variable de entorno
                    optionsBuilder.UseNpgsql("Host=localhost;Database=parkingdb;Username=postgres;Password=123456");
                }
            }
        }
    }
}
