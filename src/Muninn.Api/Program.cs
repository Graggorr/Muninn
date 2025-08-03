using Muninn.Api;
using Muninn.Api.Middlewares;
using Muninn.Kernel;
using Scalar.AspNetCore;

var builder = WebApplication.CreateSlimBuilder(args);
var services = builder.Services;

builder.WebHost.UseKestrelHttpsConfiguration();

services.AddLogging();
services.AddHttpLogging();
services.AddOpenApi();
services.AddMuninKernel();
services.AddSwaggerGen();
services.AddMvc();
services.AddControllers();
services.AddApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.MapOpenApi();
    app.MapScalarApiReference(config =>
    {
        config.DotNetFlag = true;
        config.Title = "Muninn";
        config.Theme = ScalarTheme.Purple;
    });
}

app.UseHttpsRedirection();
app.UseHttpLogging();
app.UseMiddleware<ApiKeyMiddleware>();
app.MapEndpoints();

await app.InitializeMuninAsync();
await app.RunAsync();
