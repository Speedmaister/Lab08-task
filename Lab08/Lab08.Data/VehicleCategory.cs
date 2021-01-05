namespace Lab08.Data
{
    public class VehicleCategory : Entity
    {
        public VehicleCategoryType Type { get; set; }
        public decimal DailyCost { get; set; }
        public decimal NightlyCost { get; set; }
    }
}
