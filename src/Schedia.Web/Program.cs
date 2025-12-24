using Avemepls.Auth.Bearer;
using Avemepls.Auth.Password;
using Avemepls.Identity.DataAccess;
using Microsoft.EntityFrameworkCore;
using Schedia.Web;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var configuration = builder.Configuration;

var oAuthOptions = configuration.GetSection("IdentityOptions").Get<OAuthOptions>()!;
services
    .AddMemoryCache()
    .AddJwtBearerTokenAuth(oAuthOptions)
    .AddPasswordAuth();

services.AddDataAccess((_, cfg) =>
{
    cfg
        .UseNpgsql(configuration.GetConnectionString("Schedia"), o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

var app = builder
    .Build()
    .InitializeData();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

await app.RunAsync();