using System.Threading.Tasks;

namespace Lab08.Services.Contracts
{
    public interface IParkingLotService
    {
        Task<Data.ParkingLot> Get();
        Task<Models.HoursInParking> CalculateHoursSpentInParking(Data.Vehicle vehicle);
        Task<bool> TryToRegisterCar(Data.Vehicle vehicle);
        Task<Models.HoursInParking> TryToUnregisterCar(Data.Vehicle vehicle);
    }
}