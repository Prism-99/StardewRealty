using System;
using System.Collections.Generic;
using System.Text;

namespace SDV_Realty_Core.Framework.Buildings
{
    public class NightLight
    {
        public float NightLightRadius { get; set; }
        public float OffsetX { get; set; } = 0;
        public float OffsetY { get; set; } = 0;
        public int NightLightType { get; set; } = 2;
        public string NightLightPosition { get; set; }
    }
}
