namespace BlazorApp.Components.Pages.Home;

public sealed class BucketFileDto
{
    public required string Key { get; init; }

    public required string FileName { get; init; }

    public required DateTimeOffset LastModifiedUtc { get; init; }
}
