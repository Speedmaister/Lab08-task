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

        private HoursInParking CalculateHoursSpentInParking(Data.ParkingLot parkingLot, Data.VehicleRecord vehicleRecord)
        {
            var now = currentTimeProvider.Now();
            var totalDaysSpent = (now.Date - vehicleRecord.RegistrationDate.Date).Days;
            bool isSameDay = totalDaysSpent == 0;

            // Need to calculate when same day.

            // Bottom is for total days spent above 0. Set days to 0 if equal to 1, because calculation is the same as below when 0
            // Calculate time during first day
            TimeOfRegistrationType timeOfRegistrationType;
            if (vehicleRecord.RegistrationDate.Hour < parkingLot.DailyCostStart.Hours)
            {
                timeOfRegistrationType = TimeOfRegistrationType.BeforeDaily;
            }
            else if (vehicleRecord.RegistrationDate.Hour < parkingLot.NigtlyCostStart.Hours)
            {
                timeOfRegistrationType = TimeOfRegistrationType.DuringDaily;
            }
            else
            {
                timeOfRegistrationType = TimeOfRegistrationType.AfterDaily;
            }

            var (firstDayDailyHours, firstDayNightlyHours) = GetCostingHoursForDay(timeOfRegistrationType, vehicleRecord.RegistrationDate, parkingLot);
            // Calculate time during last day
            TimeOfRegistrationType timeOfUnregistrationType;
            if (now.Hour < parkingLot.DailyCostStart.Hours)
            {
                timeOfUnregistrationType = TimeOfRegistrationType.BeforeDaily;
            }
            else if (now.Hour < parkingLot.NigtlyCostStart.Hours)
            {
                timeOfUnregistrationType = TimeOfRegistrationType.DuringDaily;
            }
            else
            {
                timeOfUnregistrationType = TimeOfRegistrationType.AfterDaily;
            }

            var (lastDayDailyHoursNotSpent, lastDayNightlyHoursNotSpent) = GetCostingHoursForDay(timeOfUnregistrationType, now, parkingLot);

            int maxDailyHoursForDay = parkingLot.NigtlyCostStart.Hours - parkingLot.DailyCostStart.Hours;
            HoursInParking hoursInParking = new HoursInParking();
            hoursInParking.Daily = firstDayDailyHours + (maxDailyHoursForDay - lastDayDailyHoursNotSpent) + totalDaysSpent * maxDailyHoursForDay;
            hoursInParking.Nightly = firstDayNightlyHours + (24 - maxDailyHoursForDay - lastDayNightlyHoursNotSpent) + totalDaysSpent * (24 - maxDailyHoursForDay);
            return hoursInParking;
        }

        private enum TimeOfRegistrationType
        {
            BeforeDaily,
            DuringDaily,
            AfterDaily
        }

        private (int dailyHours, int nightlyHours) GetCostingHoursForDay(TimeOfRegistrationType timeOfRegistrationType, DateTime registrationDate, Data.ParkingLot parkingLot)
        {
            switch (timeOfRegistrationType)
            {
                case TimeOfRegistrationType.BeforeDaily:
                    return ((parkingLot.NigtlyCostStart.Hours - parkingLot.DailyCostStart.Hours), (parkingLot.DailyCostStart.Hours - registrationDate.Hour) + (24 - parkingLot.NigtlyCostStart.Hours));
                case TimeOfRegistrationType.DuringDaily:
                    return (parkingLot.NigtlyCostStart.Hours - registrationDate.Hour, 24 - parkingLot.NigtlyCostStart.Hours);
                case TimeOfRegistrationType.AfterDaily:
                    return (0, 24 - registrationDate.Hour);
                default:
                    return (0, 0);
            }
        }
    }
}
