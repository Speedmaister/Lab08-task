using Lab08.Repository.Contracts;
using Lab08.Services.Contracts;
using Lab08.Services.Exceptions;
using Lab08.Services.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Lab08.Services
{
    public class VehicleRegistrationService : IVehicleRegistrationService
    {
        private readonly IRepository<Data.Vehicle> vehicleRepository;
        private readonly IRepository<Data.ParkingLot> parkingLotRepository;
        private readonly IRepository<Data.PromotionCard> promotionCardRepository;
        private readonly IRepository<Data.VehicleCategory> vehicleCategoryRepository;

        private readonly IParkingLotService parkingLotService;

        public VehicleRegistrationService(IRepository<Data.Vehicle> vehicleRepository,
            IRepository<Data.ParkingLot> parkingLotRepository,
            IRepository<Data.PromotionCard> promotionCardRepository,
            IRepository<Data.VehicleCategory> vehicleCategoryRepository,
            IParkingLotService parkingLotService)
        {
            this.vehicleRepository = vehicleRepository;
            this.parkingLotRepository = parkingLotRepository;
            this.promotionCardRepository = promotionCardRepository;
            this.vehicleCategoryRepository = vehicleCategoryRepository;

            this.parkingLotService = parkingLotService;
        }

        public async Task<bool> RegisterVehicleAsync(Vehicle vehicle)
        {
            var vehicleDb = await vehicleRepository.FirstOrDefaultAsync(x => x.RegistrationNumber == vehicle.RegistrationNumber);
            if (vehicleDb == null)
            {
                vehicleDb = await RegisterVehicleInDbAsync(vehicle);
            }
            else
            {
                CheckIfCategoryIsChanged(vehicle, vehicleDb);
            }

            bool isRegistered = await parkingLotService.TryToRegisterCar(vehicleDb);
            var vehicleCardType = (Data.CardType)Enum.Parse(typeof(Data.CardType), vehicle.CardType);
            if (isRegistered && vehicleDb.PromotionCard.Type != vehicleCardType)
            {
                var newCard = promotionCardRepository.FirstOrDefault(x => x.Type == vehicleCardType);
                vehicleDb.PromotionCard = newCard;
                await vehicleRepository.UpdateAsync(vehicleDb);
            }

            return isRegistered;
        }

        private static void CheckIfCategoryIsChanged(Vehicle vehicle, Data.Vehicle vehicleDb)
        {
            var categoryType = ValidateVehicleCategory(vehicle);
            if (vehicleDb.Category.Type != categoryType)
            {
                throw new ChangedVehicleCategoryException(vehicleDb.Category.Type);
            }
        }

        private async Task<Data.Vehicle> RegisterVehicleInDbAsync(Vehicle vehicle)
        {
            var categoryType = ValidateVehicleCategory(vehicle);
            var cardType = ValidateCardType(vehicle);

            var category = await vehicleCategoryRepository.FirstOrDefaultAsync(x => x.Type == categoryType);
            var card = await promotionCardRepository.FirstOrDefaultAsync(x => x.Type == cardType);

            var vehicleDb = new Data.Vehicle()
            {
                RegistrationNumber = vehicle.RegistrationNumber,
                Category = category,
                PromotionCard = card
            };

            return await vehicleRepository.InsertAsync(vehicleDb);
        }

        private static Data.CardType ValidateCardType(Vehicle vehicle)
        {
            if (!Enum.TryParse(vehicle.CardType, out Data.CardType cardType))
            {
                throw new InvalidCardTypeException(vehicle.CardType);
            }

            return cardType;
        }

        private static Data.VehicleCategoryType ValidateVehicleCategory(Vehicle vehicle)
        {
            if (!Enum.TryParse(vehicle.CategoryType, out Data.VehicleCategoryType category))
            {
                throw new InvalidVehicleCategoryException(vehicle.CategoryType);
            }

            return category;
        }
    }
}
