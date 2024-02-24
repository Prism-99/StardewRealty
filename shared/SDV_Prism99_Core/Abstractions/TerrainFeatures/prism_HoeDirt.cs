using StardewValley.TerrainFeatures;

namespace Prism99_Core.Abstractions.TerrainFeatures
{
    internal class prism_HoeDirt : HoeDirt
    {
        public HoeDirt instance;

        public prism_HoeDirt(HoeDirt instance)
        {
            this.instance = instance;
        }

        public string Fertilizer
        {
            get
            {

                return instance.fertilizer.Value;
            }
            set
            {
                 instance.fertilizer.Value=value;
            }
        }
    }
}
