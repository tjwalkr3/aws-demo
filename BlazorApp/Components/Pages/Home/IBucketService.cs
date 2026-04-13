namespace BlazorApp.Components.Pages.Home;

public interface IBucketService
{
    Task<IReadOnlyList<BucketFileDto>> ListFilesAsync(
        CancellationToken cancellationToken = default
    );

    Task UploadFileAsync(
        string fileName,
        Stream content,
        string? contentType,
        CancellationToken cancellationToken = default
    );

    Task DeleteFileAsync(string key, CancellationToken cancellationToken = default);

    Task<string> GetDownloadUrlAsync(string key, CancellationToken cancellationToken = default);
}
