using System.Collections.Generic;
using System.Linq;
using StardewValley.Objects;

namespace Prism99_Core.Abstractions.Objects
{
    internal class prism_Chest:Chest
    {
        private Chest instance;

        public prism_Chest(Chest inst)
        {
            instance= inst;
        }
        public List<Item> items
        {
            get 
            {
                return instance.Items.ToList();
            }
            set { }
        }
    }
}
