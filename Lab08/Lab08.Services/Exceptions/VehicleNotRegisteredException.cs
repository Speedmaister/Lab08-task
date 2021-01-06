using System;
using System.Collections.Generic;
using System.Text;

namespace Lab08.Services.Exceptions
{
    public class VehicleNotRegisteredException : ApplicationException
    {
        public VehicleNotRegisteredException(string registrationNumber)
        {
            this.RegistrationNumber = registrationNumber;
        }

        public override string ToString()
        {
            return $"Vehicle[{RegistrationNumber}] is not registered.";
        }

        public string RegistrationNumber { get; set; }
    }
}
