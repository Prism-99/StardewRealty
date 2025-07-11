using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using System.IO;
using System.Text;
using xTile.Dimensions;

namespace SDV_Realty_Core.Framework.Weather
{
    internal class fog : IWeatherCondition
    {
        private Texture2D fogTexture;
  
        public fog(IModContentHelper helper)
        {
            fogTexture = helper.Load<Texture2D>(Path.Combine("data", "assets", "weather", "fog.png"));
        }
        public string DisplayName() => "Fog";
        public bool IsRaining() => true;

        public void Draw(ref WeatherManager.CurrentWeather cw)
        {
            //
            //  end fog at 11:30
            //
            if (Game1.timeOfDay > 1130)
                return;

            Color fogColour = Color.LightSteelBlue;
            //
            //  set fog darkness
            //
            fogColour = Game1.timeOfDay switch
            {
                > 1120 => fogColour * 0.55f,
                > 1110 => fogColour * 0.65f,
                > 1100 => fogColour * 0.70f,
                > 1040 => fogColour * 0.75f,
                > 1030 => fogColour * 0.80f,
                > 1000 => fogColour * 0.85f,
                _ => fogColour * 0.95f
            };
            Game1.spriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, SamplerState.PointClamp);
            Game1.spriteBatch.Draw(fogTexture, new Rectangle(cw.Location.X - Game1.viewport.X, cw.Location.Y - Game1.viewport.Y, cw.Size.Width, cw.Size.Height), new Rectangle(0, 0, fogTexture.Width, fogTexture.Height), fogColour);
            Game1.spriteBatch.End();
        }

        public string Name() => "prism99.advize.stardewrealty.fog";

        public void UpdateTicked(ref WeatherManager.CurrentWeather cw)
        {
            cw.IntParam1++;
            if (cw.IntParam1 > 20)
            {
                cw.Location.X += 1;
                //cw.Location.Y += 5;
                cw.Size.Width += 5;
                cw.Size.Height += 5;
                cw.IntParam1 = 0;
            }
        }
        public void OneSecondUpdateTicked(OneSecondUpdateTickedEventArgs e, ref WeatherManager.CurrentWeather cw)
        {
     
        }

        public void StartCycle(ref WeatherManager.CurrentWeather cw)
        {
            //
            //  set the starting location and size for the fog patch
            //
            cw.Location = new Point((int)(cw.GameLocation.map.DisplayWidth * -0.3f), 0);
            cw.Size = new Size((int)(cw.GameLocation.map.DisplayWidth * 0.9f), (int)(cw.GameLocation.map.DisplayHeight * .9f));
        }

        public void TenMinuteUpdateTicked(ref WeatherManager.CurrentWeather cw)
        {

        }

        public void Update(GameTime time, ref WeatherManager.CurrentWeather cw)
        {

        }
    }
}
