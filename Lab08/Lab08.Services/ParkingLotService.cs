using Lab08.Repository.Contracts;
using Lab08.Services.Contracts;
using Lab08.Services.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lab08.Services
{
    public class ParkingLotService : IParkingLotService
    {
        private readonly IRepository<Data.ParkingLot> parkingLotRepository;

        public ParkingLotService(IRepository<Data.ParkingLot> parkingLotRepository)
        {
            this.parkingLotRepository = parkingLotRepository;
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
                    parkingLot.Vehicles = new List<Data.Vehicle>();
                }

                parkingLot.Vehicles.Add(vehicle);
                await parkingLotRepository.UpdateAsync(parkingLot);
            }

            return hasSpace;
        }
    }
}
