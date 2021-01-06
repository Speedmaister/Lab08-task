namespace Lab08.Services.Exceptions
{
    public class VehicleNotInParkingLotException : ApplicationException
    {
        public VehicleNotInParkingLotException(string registrationNumber)
        {
            this.RegistrationNumber = registrationNumber;
        }

        public override string ToString()
        {
            return $"Vehicle[{RegistrationNumber}] is not in the parking lot.";
        }

        public string RegistrationNumber { get; set; }
    }
}
