using GrpcService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Net;

namespace GrpcService.Services.Configurations
{
    public class CommandsConfigurations : IEntityTypeConfiguration<Commands>
    {
        public void Configure(EntityTypeBuilder<Commands> entity)
        {

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Command)
                .IsRequired();

            entity.Property(e => e.Parameters)
                .IsRequired();

            entity.HasOne(e => e.DeviceInformation)
                .WithMany(e => e.Commands)
                .HasForeignKey(e => e.DeviceId)
                .IsRequired();

        }
    }
}
