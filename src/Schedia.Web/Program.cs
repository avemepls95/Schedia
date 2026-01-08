using Avemepls.Infrastructure;

using Microsoft.IdentityModel.Logging;

using Schedia.Web;
using Schedia.Web.Components;
using Schedia.Web.Core.Blazor.BlazorAssembliesRegistry;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var configuration = builder.Configuration;

services
    .AddMemoryCache()
    .AddInfrastructureServices();

builder.Services.AddSchediaBase(configuration);
builder.AddSchediaAdmin();

var app = builder
    .Build()
    .InitializeData();

app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}
else
{
    IdentityModelEventSource.ShowPII = true;
}

app.UseDeveloperExceptionPage();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

if (!app.Environment.IsDevelopment())
{
    app.UseResponseCompression();
}

app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies([.. BlazorAssembliesRegistry.Assemblies]);

await app.RunAsync();