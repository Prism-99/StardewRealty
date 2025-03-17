using System;
using System.Collections.Generic;
using System.Text;

namespace SDV_Realty_Core.Framework.Objects
{
    internal class StationDetails
    {
        private string? network = null;
        public enum StationType
        {
            Train,
            Boat,
            Bus
        }
        public string Key { get; set; }
        public StationType Type { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public Point InPoint { get; set; }
        public int Cost { get; set; }
        public string Condition { get; set; } = string.Empty;
        public int FacingDirection { get; set; }       
        public string DisplayName { get; set; } = string.Empty;
        public string Network
        {
            get
            {
                if (!string.IsNullOrEmpty(network)) return network;
                return Type switch
                {
                    StationType.Train => "Train",
                    StationType.Boat => "Boat",
                    StationType.Bus => "Bus",
                    _ => "Train"
                };
            }
            set { network = value; }
        }
    }
}
