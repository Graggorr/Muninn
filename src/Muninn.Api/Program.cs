using Muninn.Kernel;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddOpenApi();
services.AddMuninKernel();

var app = builder.Build();
await app.InitializeMuninAsync();
