using System;
using System.Collections.Generic;
using System.Text;

namespace Lab08.Data
{
    public class Parking : Entity
    {
        public int TotalSpace { get; set; }
        public int AvailableSpace { get; set; }
        public TimeSpan DailyCostStart { get; set; }
        public TimeSpan NigtlyCostStart { get; set; }
        public List<Vehicle> Vehicles { get; set; }
    }
}
