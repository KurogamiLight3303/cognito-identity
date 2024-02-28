namespace CognitoPOC.Infrastructure.Configurations;

public class S3StorageConfiguration
{
    public int Quality { get; set; }
    public int BaseWidth { get; set; }
    public int BaseHeight { get; set; }
    public int SmallSquareWidth { get; set; }
    public int SmallSquareHeight { get; set; }
    public int MediumSquareWidth { get; set; }
    public int MediumSquareHeight { get; set; }
    public int UrlTimeout { get; set; }
    public string? BucketName { get; set; }
}