using Mapster;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using static System.Reflection.Assembly;
using Module = ServiceCollection.Extensions.Modules.Module;

namespace Muninn.Auction.Common;

public abstract class BaseModule(IConfiguration configuration) : Module
{
    protected readonly Assembly _assembly = GetExecutingAssembly();

    protected readonly IConfiguration _configuration = configuration;

    protected override void Load(IServiceCollection services)
    {
        TypeAdapterConfig.GlobalSettings.Scan(_assembly);
    }
}
