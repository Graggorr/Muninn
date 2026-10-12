using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Muninn.Auction.Application.Common;
using Muninn.Auction.Application.Extensions;
using Muninn.Auction.Common;

namespace Muninn.Auction.Application;

public class ApplicationModule(IConfiguration configuration) : BaseModule(configuration)
{
    protected override void Load(IServiceCollection services)
    {
        base.Load(services);
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddOrleans(builder => builder.RegisterOrleans(_configuration));
    }
}
