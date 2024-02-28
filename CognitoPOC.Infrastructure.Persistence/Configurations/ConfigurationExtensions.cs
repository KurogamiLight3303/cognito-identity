using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using CognitoPOC.Domain.Common.Models;

namespace CognitoPOC.Infrastructure.Persistence.Configurations;

public static class ConfigurationExtensions
{
    internal static EntityTypeBuilder<TDomainEntity> ConfigureVerifiable
        <TDomainEntity, TKey, TVerifiableValue>(
            this EntityTypeBuilder<TDomainEntity> builder, 
            Expression<Func<TDomainEntity, VerifiableValue<TVerifiableValue>>> property,
            Func<PropertyBuilder<TVerifiableValue>, PropertyBuilder<TVerifiableValue>> valueConversion,
            string valueColumnName, string verifiableColumnName) 
        where TDomainEntity : DomainObject<TKey>
        where TVerifiableValue : DomainValue
    {
        return builder.OwnsOne(property!, p =>
        {
            p
                .Property(s => s.Verified)
                .HasColumnName(verifiableColumnName)
                .HasDefaultValue(false)
                .IsRequired();
            valueConversion
                .Invoke(p.Property(s => s.Value!))
                .HasColumnName(valueColumnName)
                .HasMaxLength(100)
                .IsRequired(false)
                ;
        });
    }
    internal static void ConfigureDomainEntity<TDomainEntity>(
        this EntityTypeBuilder<TDomainEntity> builder,
        string? tableName = null
    )
        where TDomainEntity : DomainObject
    {
        if (!string.IsNullOrEmpty(tableName))
            builder.ToTable(tableName);
        builder.Property(p => p.CreatedDate).IsRequired();
        builder.Property(p => p.UpdatedDate).IsRequired(false);
        builder.Property(p => p.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(p => p.UpdatedBy).HasMaxLength(100).IsRequired(false);

        if (typeof(TDomainEntity).GetInterfaces().Contains(typeof(IMetadataContainer)))
        {
            builder
                .Property<MetadataDomainValue>("Metadata")
                .HasConversion(
                    p => p.ToString(), 
                    p => new(p));
        }
    }
    internal static void ConfigureDomainEntity<TDomainEntity, TKey>(
        this EntityTypeBuilder<TDomainEntity> builder,
        string? tableName = null
    )
        where TDomainEntity : DomainObject<TKey>
    {
        if (!string.IsNullOrEmpty(tableName))
            builder.ToTable(tableName);
        builder.HasKey(p => p.Id);
        builder.Property(p => p.CreatedDate).IsRequired();
        builder.Property(p => p.UpdatedDate).IsRequired(false);
        builder.Property(p => p.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(p => p.UpdatedBy).HasMaxLength(100).IsRequired(false);

        if (typeof(TDomainEntity).GetInterfaces().Contains(typeof(IMetadataContainer)))
        {
            builder
                .Property<MetadataDomainValue>("Metadata")
                .HasConversion(
                    p => p.ToString(), 
                    p => new(p));
        }
    }
    
    internal static PropertyBuilder<T?> HasJsonConversion<T>(this PropertyBuilder<T?> propertyBuilder, string? columnName = null) where T:class
    {
        var converter = new ValueConverter<T?, string?>(
            v => SerializeJson(v),
            v => DeserializeJson<T>(v));

        var comparer = new ValueComparer<T?>(
            (o1, o2) =>  SerializeJson(o1) == SerializeJson(o2),
            v => v == null ? 0 : (SerializeJson(v) ?? string.Empty).GetHashCode(),
            v => DeserializeJson<T>(SerializeJson(v)));

        propertyBuilder.HasConversion(converter, comparer);
        propertyBuilder.HasColumnType("nvarchar(max)");
        if (!string.IsNullOrEmpty(columnName))
            propertyBuilder.HasColumnName(columnName);
        return propertyBuilder;
    }

    private static string? SerializeJson<T>(T? data)
    {
        var options = new JsonSerializerOptions();
        return data != null ? JsonSerializer.Serialize(data, options) : null;
    }
    private static T? DeserializeJson<T>(string? raw) where T:class
    {
        var options = new JsonSerializerOptions();
        return raw != null ? JsonSerializer.Deserialize<T>(raw, options) : null;
    }
}