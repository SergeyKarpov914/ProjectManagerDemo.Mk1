using Acsp.Core.Lib.Data;
using Acsp.Core.Lib.Extension;
using BlazorServerTelerik.Components;
using BlazorServerTelerik.Components.Service;
using Clio.ProjectManagerModel.ViewModel.Presentation;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
//  .AddInteractiveWebAssemblyComponents();

builder.Services.AddTelerikBlazor();
builder.Services.AddScoped<ThemeService>();

#region custom container registry

builder.Services.Configure<Connection>(builder.Configuration.GetSection("BlazorConnection"));
typeof(ProjectManagerViewModelBlazor).RegisterCascading(builder.Services, () => builder.Services.AddSingleton<ProjectManagerViewModelBlazor>());

#endregion custom container registry

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
//    .AddInteractiveWebAssemblyRenderMode()
//    .AddAdditionalAssemblies(typeof(BlazorServerTelerik.Client._Imports).Assembly);

app.Run();
