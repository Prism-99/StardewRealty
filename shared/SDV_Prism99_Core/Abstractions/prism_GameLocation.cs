
namespace Prism99_Core.Abstractions
{
    internal class prism_GameLocation : GameLocation
    {
        private GameLocation instance;
        public prism_GameLocation(GameLocation org)
        {
            instance = org;
        }
        public string SeasonOverride
        {
            get { return instance.GetSeason().ToString(); }
            set { instance.seasonUpdate(); }
        }
    }
}
