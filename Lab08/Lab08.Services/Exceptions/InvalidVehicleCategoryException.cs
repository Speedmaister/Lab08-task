namespace Lab08.Services.Exceptions
{
    public class InvalidVehicleCategoryException : ApplicationException
    {
        public InvalidVehicleCategoryException(string category)
        {
            this.Category = category;
        }

        public override string ToString()
        {
            return $"The given vehicle category {Category} does not exist.";
        }

        public string Category { get; set; }
    }
}
