namespace Lab08.Services.Exceptions
{
    public class VehicleAlreadyInParkingLotException : ApplicationException
    {
        public VehicleAlreadyInParkingLotException(string registrationNumber)
        {
            this.RegistrationNumber = registrationNumber;
        }

        public override string ToString()
        {
            return $"Vehicle[{RegistrationNumber}] is already parked inside the lot.";
        }

        public string RegistrationNumber { get; set; }
    }
}
