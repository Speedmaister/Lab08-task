using Lab08.Repository.Contracts;
using Lab08.Repository.Enums;
using Lab08.Services.Contracts;
using Lab08.Services.Exceptions;
using Lab08.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lab08.Services
{
    public class ParkingLotService : IParkingLotService
    {
        private readonly IRepository<Data.ParkingLot> parkingLotRepository;
        private readonly ICurrentTimeProvider currentTimeProvider;

        public ParkingLotService(IRepository<Data.ParkingLot> parkingLotRepository,
            ICurrentTimeProvider currentTimeProvider)
        {
            this.parkingLotRepository = parkingLotRepository;
            this.currentTimeProvider = currentTimeProvider;
        }

        public async Task<Data.ParkingLot> Get()
        {
            var parkingLot = await parkingLotRepository.FirstOrDefaultAsync(x => true);
            return parkingLot;
        }

        public async Task<bool> TryToRegisterCar(Data.Vehicle vehicle)
        {
            var parkingLot = await Get();

            if (parkingLot.Vehicles != null && parkingLot.Vehicles.Any(x => x.Id == vehicle.Id))
            {
                throw new VehicleAlreadyInParkingLotException(vehicle.RegistrationNumber);
            }

            var vehicleRequiredSpace = vehicle.GetParkingSpace();
            var record = new Data.VehicleRecord()
            {
                Id = vehicle.Id,
                RegistrationDate = currentTimeProvider.Now(),
                PromotionCard = vehicle.PromotionCard,
                Category = vehicle.Category
            };

            // Prevent simultanious updates by applying simple concurrency control.
            var addVehicleToParkingUpdate = MongoDB.Driver.Builders<Data.ParkingLot>.Update.AddToSet(x => x.Vehicles, record);

            var result = CollectionUpdateResult.Conflict;
            bool hasSpace = false;
            while (result == CollectionUpdateResult.Conflict)
            {
                // Recalculate available space, because another request might have added another car
                hasSpace = 0 <= parkingLot.AvailableSpace - vehicleRequiredSpace;
                var takeAvailableSpaceUpdate = MongoDB.Driver.Builders<Data.ParkingLot>.Update.Set(x => x.AvailableSpace, parkingLot.AvailableSpace - vehicleRequiredSpace);
                if (hasSpace)
                {
                    if (parkingLot.Vehicles == null)
                    {
                        parkingLot.Vehicles = new List<Data.VehicleRecord>();
                    }

                    result = await parkingLotRepository.UpdateConcurrentlyAsync(parkingLot, null, addVehicleToParkingUpdate, takeAvailableSpaceUpdate);
                    if(result == CollectionUpdateResult.Conflict)
                    {
                        parkingLot = await Get();
                        // Vehicle might have been added by another request
                        if (parkingLot.Vehicles != null && parkingLot.Vehicles.Any(x => x.Id == vehicle.Id))
                        {
                            throw new VehicleAlreadyInParkingLotException(vehicle.RegistrationNumber);
                        }
                    }
                    else if( result == CollectionUpdateResult.Failed)
                    {
                        // Log error
                        hasSpace = false;
                        break;
                    }
                    // Nothing to do on success
                }
                else
                {
                    // No need to try to put the vehicle in the parking if its already full.
                    break;
                }
            }

            return hasSpace;
        }

        public async Task<HoursInParking> TryToUnregisterCar(Data.Vehicle vehicle)
        {
            var parkingLot = await Get();

            if (parkingLot.Vehicles == null || parkingLot.Vehicles.All(x => x.Id != vehicle.Id))
            {
                throw new VehicleNotInParkingLotException(vehicle.RegistrationNumber);
            }

            // Prevent simultanious updates by applying simple concurrency control.
            var record = parkingLot.Vehicles.FirstOrDefault(x => x.Id == vehicle.Id);
            var removeVehicleFromParkingUpdate = MongoDB.Driver.Builders<Data.ParkingLot>.Update.Pull(x => x.Vehicles, record);

            var vehicleRequiredSpace = vehicle.GetParkingSpace();

            var result = CollectionUpdateResult.Conflict;
            while (result == CollectionUpdateResult.Conflict)
            {
                var returnAvailableSpaceUpdate = MongoDB.Driver.Builders<Data.ParkingLot>.Update.Set(x => x.AvailableSpace, parkingLot.AvailableSpace + vehicleRequiredSpace);
                result = await parkingLotRepository.UpdateConcurrentlyAsync(parkingLot, null, removeVehicleFromParkingUpdate, returnAvailableSpaceUpdate);
                if(result == CollectionUpdateResult.Conflict)
                {
                    // Simply reload parking with new data and try again.
                    parkingLot = await Get();
                    // Vehicle might have been removed by another request
                    if (parkingLot.Vehicles == null || parkingLot.Vehicles.All(x => x.Id != vehicle.Id))
                    {
                        throw new VehicleNotInParkingLotException(vehicle.RegistrationNumber);
                    }
                }
                else if (result == CollectionUpdateResult.Failed)
                {
                    // Log error
                    break;
                }
                // Nothing to do on success
            }

            return CalculateHoursSpentInParking(parkingLot, record);
        }

        public async Task<HoursInParking> CalculateHoursSpentInParking(Data.Vehicle vehicle)
        {
            var parkingLot = await Get();

            if (parkingLot.Vehicles == null || parkingLot.Vehicles.All(x => x.Id != vehicle.Id))
            {
                throw new VehicleNotInParkingLotException(vehicle.RegistrationNumber);
            }

            var vehicleRecord = parkingLot.Vehicles.FirstOrDefault(x => x.Id == vehicle.Id);
            return CalculateHoursSpentInParking(parkingLot, vehicleRecord);
        }

        private HoursInParking CalculateHoursSpentInParking(Data.ParkingLot parkingLot, Data.VehicleRecord vehicleRecord)
        {
            var dailyHours = new HashSet<int>(PeriodToCollection(parkingLot.DailyCostStart, parkingLot.NigtlyCostStart - TimeSpan.FromHours(1)));
            // Use service for providing current time for easier testing.
            var now = currentTimeProvider.Now();

            int dailyHoursToPayCount = 0;
            int nightlyHoursToPayCount = 0;

            var totalDaysSpent = (now.Date - vehicleRecord.RegistrationDate.Date).Days;
            if (totalDaysSpent == 0) // left the same day
            {
                var hoursToPay = PeriodToCollection(vehicleRecord.RegistrationDate.TimeOfDay, now.TimeOfDay);
                var dailyHoursToPay = hoursToPay.Where(x => dailyHours.Contains(x));
                var nightlyHoursToPay = hoursToPay.Where(x => !dailyHours.Contains(x));

                dailyHoursToPayCount = dailyHoursToPay.Count();
                nightlyHoursToPayCount = nightlyHoursToPay.Count();
            }
            else if (totalDaysSpent == 1) // left on the next day
            {
                var hoursToPay = PeriodToCollection(vehicleRecord.RegistrationDate.TimeOfDay, TimeSpan.FromHours(23));
                hoursToPay.AddRange(PeriodToCollection(TimeSpan.Zero, now.TimeOfDay));
                var dailyHoursToPay = hoursToPay.Where(x => dailyHours.Contains(x));
                var nightlyHoursToPay = hoursToPay.Where(x => !dailyHours.Contains(x));

                dailyHoursToPayCount = dailyHoursToPay.Count();
                nightlyHoursToPayCount = nightlyHoursToPay.Count();
            }
            else // left after a few days
            {
                var hoursToPay = PeriodToCollection(vehicleRecord.RegistrationDate.TimeOfDay, TimeSpan.FromHours(23));
                hoursToPay.AddRange(PeriodToCollection(TimeSpan.Zero, now.TimeOfDay));
                var dailyHoursToPay = hoursToPay.Where(x => dailyHours.Contains(x));
                var nightlyHoursToPay = hoursToPay.Where(x => !dailyHours.Contains(x));

                dailyHoursToPayCount = dailyHoursToPay.Count() + (totalDaysSpent - 1) * dailyHours.Count;
                nightlyHoursToPayCount = nightlyHoursToPay.Count() + (totalDaysSpent - 1) * (24 - dailyHours.Count);
            }


            HoursInParking hoursInParking = new HoursInParking();
            hoursInParking.Daily = dailyHoursToPayCount;
            hoursInParking.Nightly = nightlyHoursToPayCount;
            return hoursInParking;
        }

        private List<int> PeriodToCollection(TimeSpan first, TimeSpan second)
        {
            List<int> collection = new List<int>();
            for (int i = first.Hours; i <= second.Hours; i++)
            {
                collection.Add(i);
            }

            return collection;
        }
    }
}
