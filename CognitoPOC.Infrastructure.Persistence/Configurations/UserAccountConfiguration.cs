using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CognitoPOC.Domain.Models.UserAccounts;

namespace CognitoPOC.Infrastructure.Persistence.Configurations;

public class UserAccountConfiguration : IEntityTypeConfiguration<UserAccountObject>
{
    public void Configure(EntityTypeBuilder<UserAccountObject> builder)
    {
        builder.ConfigureDomainEntity<UserAccountObject, Guid>("UserAccounts");

        builder.OwnsOne(s => s.PhoneNumber, s
            =>
        {
            s
                .Property(t => t.Verified)
                .HasColumnName("PhoneNumberConfirmed")
                .HasDefaultValue(false)
                .IsRequired();

            s.Property(t => t.Value)
                .HasColumnName("PhoneNumber")
                .HasMaxLength(100)
                .IsRequired(false)
                .HasConversion(t => t != null ? t.ToString() : null,
                    t => !string.IsNullOrEmpty(t) ? new(t) : null);
        });
        builder.Property(s => s.NewPhone).HasColumnName("NewPhone")
            .HasMaxLength(20)
            .IsRequired(false);
        builder.OwnsOne(s => s.Email, s
            =>
        {
            s
                .Property(t => t.Verified)
                .HasColumnName("EmailConfirmed")
                .HasDefaultValue(false)
                .IsRequired();

            s.Property(t => t.Value)
                .HasColumnName("Email")
                .HasMaxLength(100)
                .IsRequired(false)
                .HasConversion(t => t != null ? t.ToString() : null,
                    t => !string.IsNullOrEmpty(t) ? new(t) : null);
        });
        
        
        builder.Property(p => p.Username)
            .HasMaxLength(100);
        builder.Property(p => p.FirstName)
            .HasMaxLength(30);
        builder.Property(p => p.LastName)
            .HasMaxLength(50);

        builder.Property(p => p.IsActive)
            .HasColumnName("IsActive");
        builder.Property(p => p.MfaEnabled)
            .HasColumnName("TwoFactorEnabled");
        builder.Property(p => p.ProfilePicture)
            .HasColumnName("ProfileImage");
        builder.Property(p => p.PreferredLanguage)
            .HasColumnName("Language");
        builder.Property(p => p.MfaPreference)
            .HasColumnName("PreferredVerificationMethod");
        builder.Property(p => p.SoftwareTokenLinked)
            .HasColumnName("SoftwareTokenAssociated");
    }
}