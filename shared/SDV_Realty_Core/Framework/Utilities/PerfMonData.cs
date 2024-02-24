using System;
using System.Collections.Generic;
using System.Text;

namespace SDV_Realty_Core.Framework.Utilities
{
    internal class GamePerfMonData
    {
        public struct GameDataRecord
        {
            public int GameDay;
            public long TotalTime;
            public long Debris;
            public long UpdateMap;
            public long Sprites;
            public long TerrainStep1;
            public long TerrainStep2;
            public long LargeTerrain;
            public long ObjectsStep1;
            public long ObjectsStep2;
        }
        public static Dictionary<string, List<GameDataRecord>> PerfRecords = new Dictionary<string, List<GameDataRecord>>();

        public static void Initialize()
        {
            PerfRecords=new Dictionary<string, List<GameDataRecord>>();

        }

        public static void AddRecord(string locationName, GameDataRecord data)
        {
            if (!PerfRecords.ContainsKey(locationName))
            {
                PerfRecords.Add(locationName,new List<GameDataRecord> { data });
            }
            else
            {
                PerfRecords[locationName].Add(data);
            }
        }
        public static string FormatData(GameDataRecord data)
        {
            return $"{data.TotalTime.ToString().PadLeft(8, ' ')}" +
                $"{data.Debris.ToString().PadLeft(8, ' ')}" +
                $"{data.UpdateMap.ToString().PadLeft(8, ' ')}" +
                $"{data.Sprites.ToString().PadLeft(8, ' ')}" +
                $"{data.TerrainStep1.ToString().PadLeft(8, ' ')}" +
                $"{data.TerrainStep2.ToString().PadLeft(8, ' ')}" +
                $"{data.LargeTerrain.ToString().PadLeft(8, ' ')}" +
                $"{data.ObjectsStep1.ToString().PadLeft(8, ' ')}" +
                $"{data.ObjectsStep2.ToString().PadLeft(8, ' ')}";
        }
    }
}
