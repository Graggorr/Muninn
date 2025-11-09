using Muninn.Api.Grpc.Services;
using Muninn.Kernel;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddLogging();
services.AddHttpLogging();
services.AddOpenApi();
services.AddMuninKernel();
services.AddSwaggerGen();

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

app.MapGrpcService<MuninnService>();

await app.UseMuninnKernelAsync();
await app.RunAsync();
