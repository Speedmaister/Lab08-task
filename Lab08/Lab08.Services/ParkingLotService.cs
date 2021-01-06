using Lab08.Repository.Contracts;
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
            var hasSpace = 0 <= parkingLot.AvailableSpace - vehicleRequiredSpace;
            if (hasSpace)
            {
                parkingLot.AvailableSpace -= vehicleRequiredSpace;
                if (parkingLot.Vehicles == null)
                {
                    parkingLot.Vehicles = new List<Data.VehicleRecord>();
                }

                parkingLot.Vehicles.Add(new Data.VehicleRecord()
                {
                    Id = vehicle.Id,
                    RegistrationDate = currentTimeProvider.Now(),
                    PromotionCard = vehicle.PromotionCard,
                    Category = vehicle.Category
                });
                await parkingLotRepository.UpdateAsync(parkingLot);
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

            var vehicleRequiredSpace = vehicle.GetParkingSpace();
            parkingLot.AvailableSpace += vehicleRequiredSpace;

            var vehicleRecord = parkingLot.Vehicles.FirstOrDefault(x => x.Id == vehicle.Id);

            parkingLot.Vehicles.RemoveAll(x => x.Id == vehicle.Id);
            await parkingLotRepository.UpdateAsync(parkingLot);

            return CalculateHoursSpentInParking(parkingLot, vehicleRecord);
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
