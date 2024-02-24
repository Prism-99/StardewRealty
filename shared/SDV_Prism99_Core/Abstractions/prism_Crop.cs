using System.Collections.Generic;
using System.Linq;
using StardewValley.GameData.Crops;


namespace Prism99_Core.Abstractions
{
    internal class prism_Crop : Crop
    {
        public Crop instance;
        public prism_Crop() { }
        public prism_Crop(Crop orig)
        {
            instance = orig;
        }
        //whichForageCrop

        public new string whichForageCrop
        {
            get
            {
                return instance.whichForageCrop.Value;
            }
            set
            {
                instance.whichForageCrop.Value = value;
            }
        }
        public HarvestMethod harvestMethod
        {
            get
            {
                return instance.GetHarvestMethod();
            }
            set
            {
                instance.GetData().HarvestMethod =(HarvestMethod) value;
            }
        }
        public double chanceForExtraCrops
        {
            get
            {
                return instance.GetData()?.ExtraHarvestChance??0;
            }
            set
            {
                instance.GetData().ExtraHarvestChance = value;
            }
        }

        public float maxHarvestIncreasePerFarmingLevel
        {
            get
            {
                return instance.GetData()?.HarvestMaxIncreasePerFarmingLevel??0;
            }
            set
            {
                instance.GetData().HarvestMaxIncreasePerFarmingLevel = value;
            }
        }

        public int minHarvest
        {
            get
            {
                return instance.GetData()?.HarvestMinStack??1;
            }
            set
            {
                instance.GetData().HarvestMinStack = value;
            }

        }
        public int maxHarvest
        {
            get
            {
                return instance.GetData()?.HarvestMaxStack??1;//return instance.ha
            }
            set
            {
                instance.GetData().HarvestMaxStack = value;
            }
        }

        public string IndexOfHarvest
        {
            get
            {
                return instance.indexOfHarvest.Value;
            }
            set
            {
                instance.indexOfHarvest.Value = value;
            }
        }

        public int RegrowAfterHarvest
        {
            get { return instance.GetData()?.RegrowDays??0; }
            set
            {
                instance.GetData().RegrowDays = value;
            }
        }

        public new string indexOfHarvest
        {
            get
            {
                return instance.indexOfHarvest.Value;
            }
            set
            {
                instance.indexOfHarvest.Value = value;
            }
        }
        public List<string> seasonsToGrowIn
        {
            get { return instance.GetData().Seasons.Select(p => p.ToString()).ToList(); }
            set { instance.GetData().Seasons.AddRange(value.Select(p => GetSeasonFromString(p)).ToList()); }
        }

        private Season GetSeasonFromString(string sval)
        {
            return sval switch
            {
                "Spring" => Season.Spring,
                "Summer" => Season.Summer,
                "Fall" => Season.Fall,
                "Winter" => Season.Winter,
                _ => Season.Spring
            };
        }
    }
}
