using Lab08.Data.Attributes;

namespace Lab08.Data
{
    public enum VehicleCategoryType
    {
        [RequiredSpace(1)]
        A,
        [RequiredSpace(2)]
        B,
        [RequiredSpace(4)]
        C
    }
}
