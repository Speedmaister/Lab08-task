using Lab08.Repository.Configuration;
using Lab08.Repository.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Lab08.Repository
{
    public static class ServiceCollectionExtensions
    {
            public static IServiceCollection AddRepository(this IServiceCollection services, IConfiguration configuration)
            {
                services.TryAddSingleton<MongoDbConfiguration>(x => configuration.GetSection("MongoDB").Get<MongoDbConfiguration>());
                services.TryAddScoped<MongoClientProvider>();
                services.TryAddTransient(typeof(IRepository<>), typeof(Repository<>));

                return services;
            }
        }
    }
