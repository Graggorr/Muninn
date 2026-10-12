using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Muninn.Auction.Common;
using Muninn.Auction.Data.Contracts;
using Muninn.Auction.Data.Extensions;
using Muninn.Auction.Data.Repositories;

namespace Muninn.Auction.Data;

public class DataModule(IConfiguration configuration) : BaseModule(configuration)
{
    protected override void Load(IServiceCollection services)
    {
        base.Load(services);
        services.RegisterNpgsql();
        services.AddScoped<ILotRepository, LotRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
    }
}
