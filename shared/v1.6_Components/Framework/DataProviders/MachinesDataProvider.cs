using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.CustomEntities;
using StardewModdingAPI.Events;
using StardewValley.GameData.Machines;


namespace SDV_Realty_Core.Framework.DataProviders
{
    /// <summary>
    /// Edits Data/Machines to add production info of SDR custom machines
    /// Uses CustomMachineManager for source data.
    /// Checks if Automate is installed and if it is not
    /// removes any Automate only recipes.
    /// </summary>
    internal class MachinesDataProvider : IGameDataProvider
    {
        private ICustomMachineDataService _machineDataService;
        private IUtilitiesService _utilitiesService;
         public MachinesDataProvider(ICustomMachineDataService machineDataService, IUtilitiesService utilitiesService)
        {
            _machineDataService = machineDataService;
            _utilitiesService = utilitiesService;
        }

        public override string Name => "Data/Machines";

        public override void CheckForActivations()
        {
            
        }

        public override void HandleEdit(AssetRequestedEventArgs e)
        {
            e.Edit(asset =>
            {
                foreach (var machine in _machineDataService.Machines.Values)
                {
                    //if (!FEFramework.IsAutomateInstalled())
                    //{
                    //    //
                    //    //  remove Automate only recipes
                    //    //
                    //    if (machine.AutomateOnlyRecipes != null)
                    //    {
                    //        foreach (string automate in machine.AutomateOnlyRecipes)
                    //        {
                    //            machine.MachineData.OutputRules.RemoveAll(p => p.Id == automate);
                    //        }
                    //    }
                    //}
                    //
                    //  add machine to game data
                    //
                    asset.AsDictionary<string, MachineData>().Data.Add(machine.MachineId, machine.MachineData);
                }
            });
        }

        public override void OnGameLaunched()
        {
            _utilitiesService.InvalidateCache(Name);
        }
    }
}
