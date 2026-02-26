using GrpcService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GrpcService.Services.Configurations
{
    public class DeviceTelemetryConfiguration : IEntityTypeConfiguration<DeviceTelemetry>
    {
        public void Configure(EntityTypeBuilder<DeviceTelemetry> entity)
        {

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Temperature)
                .IsRequired();

            entity.Property(e => e.Humidity)
                .IsRequired();

            entity.Property(e => e.Timestamp)
                .HasColumnType("datetime")
                .IsRequired();

            entity.HasOne(e => e.DeviceInformation)
                .WithMany(e => e.DeviceTelemetry)
                .HasForeignKey(e => e.DeviceId)
                .IsRequired();

        }
    }
}
