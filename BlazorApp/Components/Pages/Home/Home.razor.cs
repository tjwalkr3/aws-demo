using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace BlazorApp.Components.Pages.Home;

public partial class Home : ComponentBase
{
    private const long MaxUploadBytes = 50 * 1024 * 1024;

    [Inject]
    private IBucketService BucketService { get; set; } = default!;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = default!;

    private IReadOnlyList<BucketFileDto> _files = [];
    private bool _isLoading;
    private string? _errorMessage;
    private string? _statusMessage;

    protected IReadOnlyList<BucketFileDto> Files => _files;
    protected bool IsLoading => _isLoading;
    protected string? ErrorMessage => _errorMessage;
    protected string? StatusMessage => _statusMessage;

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
            _isLoading = true;
            _errorMessage = null;
            _statusMessage = $"Uploading {file.Name}...";
            StateHasChanged();
            await using var stream = file.OpenReadStream(MaxUploadBytes);
            await BucketService.UploadFileAsync(file.Name, stream, file.ContentType);
            _files = await BucketService.ListFilesAsync();
            _statusMessage = $"Uploaded {file.Name}.";
        }
        catch (Exception ex)
        {
            _errorMessage = ex.Message;
            _statusMessage = null;
        }
        finally
        {
            _isLoading = false;
        }
    }

    protected async Task DeleteAsync(string key)
    {
        try
        {
            _isLoading = true;
            _errorMessage = null;
            _statusMessage = "Deleting file...";
            await BucketService.DeleteFileAsync(key);
            _files = await BucketService.ListFilesAsync();
            _statusMessage = "File deleted.";
        }
        catch (Exception ex)
        {
            _errorMessage = ex.Message;
            _statusMessage = null;
        }
        finally
        {
            _isLoading = false;
        }
    }

    protected async Task DownloadAsync(string key)
    {
        try
        {
            _errorMessage = null;
            var url = await BucketService.GetDownloadUrlAsync(key);
            await JsRuntime.InvokeVoidAsync("open", url, "_blank");
        }
        catch (Exception ex)
        {
            _errorMessage = ex.Message;
        }
    }
}
