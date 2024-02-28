using CognitoPOC.Domain.Common.Models;

namespace CognitoPOC.Domain.Models.Pictures;

public class PictureResume : DomainResume<PictureObject>
{
    public Guid Id { get; init; }
    public string? PreviewUrl { get; init; }
    public DateTime UploadDate { get; init; }
}