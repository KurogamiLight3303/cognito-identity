using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CognitoPOC.Domain.Models.Pictures;

namespace CognitoPOC.Infrastructure.Persistence.Configurations;

public class PicturesConfiguration : IEntityTypeConfiguration<PictureObject>
{
    public void Configure(EntityTypeBuilder<PictureObject> builder)
    {
        builder.ConfigureDomainEntity<PictureObject, Guid>("Pictures");
    }
}