using BlazorApp.Components;
using BlazorApp.Components.Pages.Home;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

var dataProtectionKeysPath =
    builder.Configuration["DATA_PROTECTION_KEYS_PATH"] ?? "/home/app/.aspnet/DataProtection-Keys";

Directory.CreateDirectory(dataProtectionKeysPath);

builder
    .Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath))
    .SetApplicationName("BlazorApp");

builder.Services.AddScoped<IBucketService, BucketService>();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
