using GrpcService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GrpcService.Services.Configurations
{
    public class DeviceInformationConfiguration : IEntityTypeConfiguration<DeviceInformation>
    {
        public void Configure(EntityTypeBuilder<DeviceInformation> entity)
        {

            entity.HasKey(e => e.DeviceId);

            entity.Property(e => e.DeviceType)
                .IsRequired();

            entity.Property(e => e.Location)
                .IsRequired();

            entity.Property(e => e.Online)
                .HasDefaultValue(false);



        }
    }
}
