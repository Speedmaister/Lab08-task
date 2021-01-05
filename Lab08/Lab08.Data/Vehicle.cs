using Lab08.Data.Attributes;
using System.Linq;

namespace Lab08.Data
{
    public class Vehicle : Entity
    {
        public string RegistrationNumber { get; set; }

        public PromotionCard PromotionCard { get; set; }

        public VehicleCategory Category { get; set; }

        public int GetParkingSpace()
        {
            var enumType = typeof(VehicleCategoryType);
            var memberInfos = enumType.GetMember(Category.Type.ToString());
            var enumValueMemberInfo = memberInfos.FirstOrDefault(m => m.DeclaringType == enumType);
            var requiredSpaceAttribute =
                  enumValueMemberInfo.GetCustomAttributes(typeof(RequiredSpaceAttribute), false).FirstOrDefault() as RequiredSpaceAttribute;
            if (requiredSpaceAttribute == null)
            {
                throw new System.Exception($"Missing {nameof(RequiredSpaceAttribute)} for category.");
            }

            return requiredSpaceAttribute.TakenPlaces;
        }
    }
}
