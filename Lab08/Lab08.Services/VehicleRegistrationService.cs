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
        private static object vehicleCreationLock = new object();

        private readonly IRepository<Data.Vehicle> vehicleRepository;
        private readonly IRepository<Data.PromotionCard> promotionCardRepository;
        private readonly IRepository<Data.VehicleCategory> vehicleCategoryRepository;

        private readonly IParkingLotService parkingLotService;

        public VehicleRegistrationService(IRepository<Data.Vehicle> vehicleRepository,
            IRepository<Data.PromotionCard> promotionCardRepository,
            IRepository<Data.VehicleCategory> vehicleCategoryRepository,
            IParkingLotService parkingLotService)
        {
            this.vehicleRepository = vehicleRepository;
            this.promotionCardRepository = promotionCardRepository;
            this.vehicleCategoryRepository = vehicleCategoryRepository;

            this.parkingLotService = parkingLotService;
        }

        public async Task<bool> RegisterVehicleAsync(Vehicle vehicle)
        {
            var vehicleDb = GetVehicleFromDbAsync(vehicle);

            bool isRegistered = await parkingLotService.TryToRegisterCar(vehicleDb);
            if (isRegistered)
            {
                Data.PromotionCard newCard = null;
                bool hasChangedCard = false;
                if (vehicleDb.PromotionCard != null && vehicle.CardType == null)// had a card but now doesn't
                {
                    newCard = null;
                    hasChangedCard = true;
                }
                else if (vehicleDb.PromotionCard == null && vehicle.CardType != null) // didn't have a card but now has
                {
                    var vehicleCardType = (Data.CardType)Enum.Parse(typeof(Data.CardType), vehicle.CardType);
                    newCard = promotionCardRepository.FirstOrDefault(x => x.Type == vehicleCardType);
                    hasChangedCard = true;
                }
                else
                {
                    var vehicleCardType = (Data.CardType)Enum.Parse(typeof(Data.CardType), vehicle.CardType);
                    if (vehicleDb.PromotionCard.Type != vehicleCardType) // had a card and now it's changed
                    {
                        newCard = promotionCardRepository.FirstOrDefault(x => x.Type == vehicleCardType);
                        hasChangedCard = true;
                    }
                }

                if (hasChangedCard)
                {
                    vehicleDb.PromotionCard = newCard;
                    await vehicleRepository.UpdateAsync(vehicleDb);
                }
            }

            return isRegistered;
        }

        private Data.Vehicle GetVehicleFromDbAsync(Vehicle vehicle)
        {
            lock (vehicleCreationLock) // Prevent multiple request to create the same vehicle in the db
            {
                var vehicleDb = vehicleRepository.FirstOrDefault(x => x.RegistrationNumber == vehicle.RegistrationNumber);
                if (vehicleDb == null)
                {
                    vehicleDb = RegisterVehicleInDbAsync(vehicle).Result;
                }
                else
                {
                    CheckIfCategoryIsChanged(vehicle, vehicleDb);
                }

                return vehicleDb;
            }
        }

        public async Task<decimal> UnregisterVehicleAsync(string registrationNumber)
        {
            var vehicleDb = await GetVehicle(registrationNumber);
            var timeSpentInParking = await parkingLotService.TryToUnregisterCar(vehicleDb);
            return CalculatePaymentAmount(vehicleDb, timeSpentInParking);
        }

        public async Task<decimal> PaymentCheckAsync(string registrationNumber)
        {
            var vehicleDb = await GetVehicle(registrationNumber);
            var timeSpentInParking = await parkingLotService.CalculateHoursSpentInParking(vehicleDb);
            return CalculatePaymentAmount(vehicleDb, timeSpentInParking);
        }

        private decimal CalculatePaymentAmount(Data.Vehicle vehicleDb, HoursInParking timeSpentInParking)
        {
            var discount = vehicleDb.PromotionCard == null ? 0 : vehicleDb.PromotionCard.Discount;
            return (timeSpentInParking.Daily * vehicleDb.Category.DailyCost + timeSpentInParking.Nightly * vehicleDb.Category.NightlyCost) * (1 - discount);
        }

        private void CheckIfCategoryIsChanged(Vehicle vehicle, Data.Vehicle vehicleDb)
        {
            var categoryType = ValidateVehicleCategory(vehicle);
            if (vehicleDb.Category.Type != categoryType)
            {
                throw new ChangedVehicleCategoryException(vehicleDb.Category.Type);
            }
        }

        private async Task<Data.Vehicle> GetVehicle(string registrationNumber)
        {
            var vehicleDb = await vehicleRepository.FirstOrDefaultAsync(x => x.RegistrationNumber == registrationNumber);
            if (vehicleDb == null)
            {
                throw new VehicleNotRegisteredException(registrationNumber);
            }

            return vehicleDb;
        }

        private async Task<Data.Vehicle> RegisterVehicleInDbAsync(Vehicle vehicle)
        {
            var categoryType = ValidateVehicleCategory(vehicle);
            var cardType = ValidateCardType(vehicle);

            var category = await vehicleCategoryRepository.FirstOrDefaultAsync(x => x.Type == categoryType);
            var card = cardType == null ? null : await promotionCardRepository.FirstOrDefaultAsync(x => x.Type == cardType);

            var vehicleDb = new Data.Vehicle()
            {
                RegistrationNumber = vehicle.RegistrationNumber,
                Category = category,
                PromotionCard = card
            };

            return await vehicleRepository.InsertAsync(vehicleDb);
        }

        private static Data.CardType? ValidateCardType(Vehicle vehicle)
        {
            if (vehicle.CardType == null)
            {
                return null;
            }

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
