using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using StardewValley.Buildings;

namespace Prism99_Core.Abstractions.Buildings
{
    internal class prism_Building : Building
    {
        public Building instance;

        public prism_Building(Building bld)
        {
            instance = bld;
        }


#if v16
        public string NameOfIndoors
        {
            get { return instance.GetIndoorsName(); }
            set { }
        }

#else
   public string NameOfIndoors
        {
            get { return instance.nameOfIndoors; }
            set { }
        }
#endif
    }
}