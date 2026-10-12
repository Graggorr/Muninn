using Mapster;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Module = ServiceCollection.Extensions.Modules.Module;

namespace Muninn.Auction.Common;

public abstract class BaseModule(IConfiguration configuration) : Module
{
    protected readonly IConfiguration _configuration = configuration;

    protected Assembly ModuleAssembly => GetType().Assembly;

    protected override void Load(IServiceCollection services)
    {
        TypeAdapterConfig.GlobalSettings.Scan(ModuleAssembly);
    }
}
