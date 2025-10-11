using Muninn.Api.Grpc.Services;
using Muninn.Kernel;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
services.AddMuninKernel();

var app = builder.Build();

app.MapGrpcService<MuninnService>();

app.Run();
