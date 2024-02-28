using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Formats.Png;
using CognitoPOC.Domain.Common.Models;
using CognitoPOC.Domain.Models.Pictures;
using CognitoPOC.Domain.Services;
using CognitoPOC.Infrastructure.Configurations;

namespace CognitoPOC.Infrastructure.Services;

public class ResourceService(IAmazonS3 s3Client, IOptions<S3StorageConfiguration> configuration, ILogger logger)
    : IResourceService
{
    private readonly S3StorageConfiguration _configuration = configuration.Value;

    public string? GetPicture(string? key, PictureSizeEnum size)
    {
        try
        {
            if (string.IsNullOrEmpty(key))
                return null;
            return s3Client.GetPreSignedURL(new GetPreSignedUrlRequest()
            {
                BucketName = _configuration.BucketName,
                Key = $"{key.ToLower()}-{size}",
                Expires = DateTime.UtcNow.AddHours(_configuration.UrlTimeout)
            });
        }
        catch (Exception exc)
        {
            logger.LogError(exc, "Unable to Get {Key} {Size}", key, size);
            return null;
        }
    }
    public async ValueTask<OperationResultValue> UploadPictureAsync(string key, Stream data, CancellationToken cancellationToken)
    {
        var baseData = new MemoryStream();
        await data.CopyToAsync(baseData, cancellationToken);
        var result = await PushAsync($"{key}-{PictureSizeEnum.Base}",
            _configuration.BaseWidth, _configuration.BaseHeight, baseData,
            cancellationToken);
        if (!result.Success)
            return result;
        result = await PushAsync($"{key}-{PictureSizeEnum.Small}",
            _configuration.SmallSquareWidth, _configuration.SmallSquareHeight, baseData,
            cancellationToken);
        if (!result.Success)
            return result;
        result =await PushAsync($"{key}-{PictureSizeEnum.Medium}",
            _configuration.MediumSquareWidth, _configuration.MediumSquareHeight,
            baseData,
            cancellationToken);
        return result;
    }
    
    private async ValueTask<OperationResultValue> PushAsync(string key, int width, int height, Stream data,
        CancellationToken cancellationToken = default)
    {
        OperationResultValue answer;
        try
        {
            data.Position = 0;
            using (var image = await Image.LoadAsync(data , cancellationToken))
            {
                var img = image.Clone(p => p.Resize(width, height));
                var mStream = new MemoryStream();
                await img.SaveAsync(mStream, new PngEncoder(), cancellationToken);
                var request = UploadRequest(key, _configuration.BucketName,mStream);
                var fileTransferUtility = new TransferUtility(s3Client);
                await fileTransferUtility.UploadAsync(request, cancellationToken);
            }
            answer = new OperationResultValue(true, "Resource loaded");
        }
        catch (Exception exc)
        {
            logger.LogError(exc, "Unable to upload {Key}", key);
            answer = new OperationResultValue(false, "Resource not loaded");
        }

        return answer;
    }
    
    private TransferUtilityUploadRequest? UploadRequest(string key, string? bucket, Stream data, string contentType = "image/jpeg", bool autoCloseStream = true)
    {
        try
        {
            return new TransferUtilityUploadRequest
            {
                InputStream = data,
                Key = key,
                BucketName = bucket,
                ContentType = contentType,
                AutoCloseStream = autoCloseStream,
                CannedACL = S3CannedACL.BucketOwnerFullControl
            };
        }
        catch (Exception exc)
        {
            logger.LogError(exc, "Unable to upload {Key}", key);
            return null;
        }
    }
}