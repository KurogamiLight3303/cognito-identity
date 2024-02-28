using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CognitoPOC.Domain.Models.UserAccounts;

namespace CognitoPOC.Infrastructure.Persistence.Configurations;

public class UserDevicesConfiguration : IEntityTypeConfiguration<UserDevicesObject>
{
    public void Configure(EntityTypeBuilder<UserDevicesObject> builder)
    {
        builder.ConfigureDomainEntity<UserDevicesObject, Guid>("Devices");
        builder.OwnsOne(p => p.DeviceInfo, p
            =>
        {
            p
                .Property(s => s.Name)
                .HasColumnName("DeviceName")
                .HasMaxLength(200);
            p
                .Property(s => s.IpAddress)
                .HasColumnName("LastIpAddress")
                .HasMaxLength(20);
            p
                .Property(s => s.UserAgent)
                .HasColumnName("DeviceUserAgent");
        });

        builder.Property<Guid>("UserId")
            .IsRequired();
        builder
            .HasOne(p => p.User)
            .WithMany(p => p.Devices)
            .HasForeignKey("UserId");
        builder.Property(p => p.IsActive)
            .HasColumnName("IsActive");
    }
}