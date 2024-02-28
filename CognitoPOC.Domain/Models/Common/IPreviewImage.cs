namespace CognitoPOC.Domain.Models.Common;

public interface IPreviewImage
{
    public string? PreviewImageId { get; }
    public string? PreviewImageUrl { get; set; }
}