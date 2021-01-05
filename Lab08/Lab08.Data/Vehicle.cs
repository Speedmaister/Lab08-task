namespace Lab08.Data
{
    public class Vehicle : Entity
    {
        public string RegistrationNumber { get; set; }

        public PromotionCard PromotionCard { get; set; }

        public VehicleCategory Category { get; set; }
    }
}
