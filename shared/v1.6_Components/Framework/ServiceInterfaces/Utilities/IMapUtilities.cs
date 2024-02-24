using Prism99_Core.Utilities;
using xTile;
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.Utilities
{
    internal abstract class IMapUtilities : IService
    {
        public const string northSidePatch = "0";
        public const string eastSidePatch = "1";
        public const string southSidePatch = "2";
        public const string westSidePatch = "3";
   
        internal abstract int GetTileSheetId(Map map, string tileSheetSourceName);
        internal abstract bool IsSeasonalTileSheet(string tileSheetSourceName);
        internal abstract string WildCardSeason(string tileSheetSourceName);
        internal abstract void SetTile(Map map, int x, int y, int iTileSheetId, int iTileId, string sLayerId, bool overwriteTile);
        internal abstract void SetTile(Map map, int x, int y, int iTilesheetId, int iTileId, string sLayerId);
        internal abstract void SetTileProperty(GameLocation gl, int x, int y, string sLayerId, string propName, string propValue);
        internal abstract void RemoveTileProperty(GameLocation gl, int x, int y, string sLayerId, string propName);
        internal abstract void PatchInMap(Map gl, Map oMap, Vector2 vOffset);
        internal abstract void PatchInMap(GameLocation gl, Map oMap, Vector2 vOffset);
        internal abstract void RemovePathBlock(Vector2 oBlockPos, GameLocation gl, WarpOrientations eBlockOrientation, int iBlockWidth);
        internal abstract void RemoveTile(GameLocation gl, int x, int y, string sLayer, int tileIndex);
        internal abstract void RemoveTile(GameLocation gl, int x, int y, string sLayer);
        internal abstract string GetNeighbourSide(string side);
        internal abstract string FormatDirectionText(string direction, string text);
        internal abstract void AddPathBlock(Vector2 oBlockPos, Map map, WarpOrientations eBlockOrientation, int iBlockWidth);
        internal abstract string GetSignPostMessagePrefix(string side);
        internal abstract string GetSignPostMessagePrefix(EntranceDirection side);

        internal abstract void RemoveSignPost(GameLocation oExp, EntrancePatch oExpPatch, string messageKeySuffix);
        internal abstract void RemovePatchWarps(EntrancePatch oPatch, GameLocation glExp);

    }
}
