using Lab08.Data;
using System.Threading.Tasks;

namespace Lab08.Services.Contracts
{
    public interface IParkingLotService
    {
        Task<ParkingLot> Get();
        Task<bool> TryToRegisterCar(Vehicle vehicle);
    }
}