using System;

namespace Lab08.Data.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class RequiredSpaceAttribute : Attribute
    {
        public RequiredSpaceAttribute(int takenPlaces)
        {
            this.TakenPlaces = takenPlaces;
        }

        public int TakenPlaces { get; set; }
    }
}
