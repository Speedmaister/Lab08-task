using Lab08.Data;
using Lab08.Repository;
using Lab08.Repository.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Lab08.DbInitialization
{
    class Program
    {
        static void Main(string[] args)
        {
            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
                  .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var configuration = configurationBuilder.Build();
            var host = Host.CreateDefaultBuilder(args)
               .ConfigureServices((context, services) =>
               {
                   services.AddRepository(configuration);
               }).Build();

            var vehicleCategoryRepository = host.Services.GetService<IRepository<VehicleCategory>>();
            vehicleCategoryRepository.InsertManyAsync(new VehicleCategory[]{
                new VehicleCategory()
                {
                    Type = VehicleCategoryType.A,
                    DailyCost = 3,
                    NightlyCost = 2
                },
                new VehicleCategory()
                {
                    Type = VehicleCategoryType.B,
                    DailyCost = 6,
                    NightlyCost = 4
                },
                new VehicleCategory()
                {
                    Type = VehicleCategoryType.C,
                    DailyCost = 12,
                    NightlyCost = 8
                }
            }).Wait();

            var promotionalCardRepository = host.Services.GetService<IRepository<PromotionCard>>();
            promotionalCardRepository.InsertManyAsync(new PromotionCard[]
            {
                new PromotionCard(){Type = CardType.Silver, Discount = 10},
                new PromotionCard(){Type = CardType.Gold, Discount = 15},
                new PromotionCard(){Type = CardType.Platinum, Discount = 20}
            }).Wait();

            var parkingRepository = host.Services.GetService<IRepository<ParkingLot>>();
            parkingRepository.InsertAsync(new ParkingLot()
            {
                AvailableSpace = 200,
                TotalSpace = 200,
                DailyCostStart = TimeSpan.FromHours(8),
                NigtlyCostStart = TimeSpan.FromHours(18)
            }).Wait();
        }
    }
}
