using GrpcService.Models;
using Microsoft.EntityFrameworkCore;

namespace GrpcService.Services
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<Commands> Commands { get; set; }
        public DbSet<DeviceInformation> DeviceInformation { get; set; }
        public DbSet<DeviceTelemetry> DeviceTelemetry { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
