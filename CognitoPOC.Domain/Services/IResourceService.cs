using CognitoPOC.Domain.Models.Pictures;
using CognitoPOC.Domain.Common.Models;

namespace CognitoPOC.Domain.Services;

public interface IResourceService
{
    string? GetPicture(string? key, PictureSizeEnum size);
    ValueTask<OperationResultValue> UploadPictureAsync(string key, Stream data, CancellationToken cancellationToken);
}