using StardewValley;
using System;
using System.Collections.Generic;
using System.Text;

namespace SDV_Realty_Core.Framework.Weather
{
    internal class WeatherCycle
{
      public  string Name { get; set; }
        public List<Season> Seasons { get; set; }
        public List<WeatherManager.WeatherPhase> Phases { get; set; }
    }
}
