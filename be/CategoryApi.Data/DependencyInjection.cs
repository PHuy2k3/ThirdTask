using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CategoryApi.Data.Repositories;

namespace CategoryApi.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddData(this IServiceCollection services, IConfiguration cfg)
    {
        var provider = (cfg["DbProvider"] ?? "SqlServer").ToLowerInvariant();
        var cs = cfg.GetConnectionString("DefaultConnection")
                 ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection");

        services.AddDbContext<AppDbContext>(opt =>
        {
            if (provider == "mysql") opt.UseMySql(cs, ServerVersion.AutoDetect(cs));
            else opt.UseSqlServer(cs);
        });

        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        return services;
    }
}
