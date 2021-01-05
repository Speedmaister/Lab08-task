using Lab08.Data;
using System;

namespace Lab08.Services.Exceptions
{
    public class ChangedVehicleCategoryException : ApplicationException
    {
        public ChangedVehicleCategoryException(VehicleCategoryType category)
        {
            this.Category = category;
        }

        public override string ToString()
        {
            return $"The given vehicle is already registered as {Category} category.";
        }

        public VehicleCategoryType Category { get; set; }
    }
}
