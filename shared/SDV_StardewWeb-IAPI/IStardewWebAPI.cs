using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley.Locations;
using StardewValley.GameData;
using StardewValley;

namespace StardewWeb
{
    public interface IStardewWebAPI
    {
        string GetMetaId(int eType, int iId);
        string GetObjectName(int iObjectId);
        string GetObjectName(int eType, int Id, string sDefault);
#if v16
        void WriteString(string fontName, int rotation, string text,GameLocation   gl, int x, int y, string fillObjectId, string strokeObjectId, bool hoeAllDirt = true, bool matureCrop = true);
#else
        void WriteString(string fontName, int rotation, string text, BuildableGameLocation  gl, int x, int y, string fillObjectId, string strokeObjectId, bool hoeAllDirt = true, bool matureCrop = true);
#endif
    }
}
