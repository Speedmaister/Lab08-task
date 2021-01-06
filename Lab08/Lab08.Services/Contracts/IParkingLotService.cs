using System.Threading.Tasks;

namespace Lab08.Services.Contracts
{
    public interface IParkingLotService
    {
        Task<Data.ParkingLot> Get();
        Task<bool> TryToRegisterCar(Data.Vehicle vehicle);
        Task<Models.HoursInParking> TryToUnregisterCar(Data.Vehicle vehicle);
    }
}