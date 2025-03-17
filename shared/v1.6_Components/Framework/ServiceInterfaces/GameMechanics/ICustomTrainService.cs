using SDV_Realty_Core.Framework.ServiceInterfaces.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.GameMechanics
{
    internal abstract class ICustomTrainService : IService
    {
        public struct DropRange
        {
            public int MinPos;
            public int MaxPos;
        }
        public class TrainDetails
        {
            public string LocationName;
            public string ApproachingMessage;
            public int TrainTime;
            public float CrossingY;
            public List<DropRange> DropZones;
            public bool TrainToday;
        }
        public override Type ServiceType => typeof(ICustomTrainService);
        public static Dictionary<string, TrainDetails> Trains { get; set; } = new();
        public abstract void setTrainComing(string locationName, int delay,bool forceActive=false);
        public void AddTrainLocation(TrainDetails details) 
        { 
            Trains[details.LocationName] = details;
        }
    }
}
