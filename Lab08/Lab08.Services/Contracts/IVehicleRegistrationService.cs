using Lab08.Services.Models;
using System.Threading.Tasks;

namespace Lab08.Services.Contracts
{
    public interface IVehicleRegistrationService
    {
        Task<bool> RegisterVehicleAsync(Vehicle vehicle);
        Task<decimal> UnregisterVehicleAsync(string registrationNumber);
        Task<decimal> PaymentCheckAsync(string registrationNumber);
    }
}
