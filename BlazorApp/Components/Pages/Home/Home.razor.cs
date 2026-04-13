using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorApp.Components.Pages.Home;

public partial class Home : ComponentBase
{
    private const long MaxUploadBytes = 50 * 1024 * 1024;

    [Inject]
    private IBucketService BucketService { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    private IReadOnlyList<BucketFileDto> _files = [];
    private bool _isLoading;
    private string? _errorMessage;

    protected IReadOnlyList<BucketFileDto> Files => _files;
    protected bool IsLoading => _isLoading;
    protected string? ErrorMessage => _errorMessage;

    protected override async Task OnInitializedAsync()
    {
        await RefreshAsync();
    }

    protected async Task RefreshAsync()
    {
        try
        {
            _isLoading = true;
            _errorMessage = null;
            _files = await BucketService.ListFilesAsync();
        }
        catch (Exception ex)
        {
            _errorMessage = ex.Message;
        }
        finally
        {
            _isLoading = false;
        }
    }

    protected async Task UploadAsync(InputFileChangeEventArgs args)
    {
        var file = args.File;
        if (file is null)
        {
            return;
        }

        try
        {
            _errorMessage = null;
            await using var stream = file.OpenReadStream(MaxUploadBytes);
            await BucketService.UploadFileAsync(file.Name, stream, file.ContentType);
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            _errorMessage = ex.Message;
        }
    }

    protected async Task DeleteAsync(string key)
    {
        try
        {
            _errorMessage = null;
            await BucketService.DeleteFileAsync(key);
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            _errorMessage = ex.Message;
        }
    }

    protected async Task DownloadAsync(string key)
    {
        try
        {
            _errorMessage = null;
            var url = await BucketService.GetDownloadUrlAsync(key);
            NavigationManager.NavigateTo(url, forceLoad: true);
        }
        catch (Exception ex)
        {
            _errorMessage = ex.Message;
        }
    }
}
