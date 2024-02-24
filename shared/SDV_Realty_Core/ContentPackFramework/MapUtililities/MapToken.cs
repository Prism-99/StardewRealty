using System.Collections.Generic;


namespace ContentPackFramework.MapUtililities
{
    internal class MapToken
    {
        public Point Position { get; set; }
        public string TokenName { get; set; }
        public string TokenValue { get; set; }
        public Dictionary<string, string> TokenProperties { get; set; } = new Dictionary<string, string>();
    }
}
