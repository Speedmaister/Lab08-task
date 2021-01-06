using Lab08.Services.Models;

namespace Lab08.Task.ViewModels.VehicleRegistration
{
    public class VehicleRegisterViewModel
    {
        public string RegistrationNumber { get; set; }
        public string CardType { get; set; }
        public string CategoryType { get; set; }

        public Vehicle ToModel()
        {
            return new Vehicle()
            {
                RegistrationNumber = this.RegistrationNumber,
                CardType = this.CardType,
                CategoryType = this.CategoryType
            };
        }
    }
}
