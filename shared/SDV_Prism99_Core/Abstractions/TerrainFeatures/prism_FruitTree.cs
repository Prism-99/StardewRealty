using System.Linq;
using StardewValley.TerrainFeatures;

namespace Prism99_Core.Abstractions.TerrainFeatures
{
    internal class prism_FruitTree : FruitTree
    {
        public FruitTree instance;

        public prism_FruitTree(FruitTree instance)
        {
            this.instance = instance;
        }

        public int fruitsOnTree
        {
            get { return instance.fruit.Count; }
            set { }
        }
        public int indexOfFruit
        {
            get { return instance.fruit.First().ParentSheetIndex; }
            set { }
        }
    }
}
