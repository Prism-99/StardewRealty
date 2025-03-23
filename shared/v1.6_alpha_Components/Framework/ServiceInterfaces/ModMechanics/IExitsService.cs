using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics
{
    internal abstract class IExitsService:IService
{
        /// <summary>
        /// Handles adding and removing exits on Expansions
        /// </summary>
        public override Type ServiceType => typeof(IExitsService);

        public abstract void AddFarmExits();
        public abstract void AddMapExitBlockers(int iGridId);
        internal abstract string GetNeighbourExpansionTTId(int iGridId, EntranceDirection side);


    }
}
