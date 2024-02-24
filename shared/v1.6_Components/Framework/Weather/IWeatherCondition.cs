using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace SDV_Realty_Core.Framework.Weather
{
    internal interface IWeatherCondition
    {
        public abstract string Name();
        public abstract string DisplayName();
        public abstract void Draw(ref WeatherManager.CurrentWeather cw);
        public abstract void StartCycle(ref WeatherManager.CurrentWeather cw);
        public abstract void Update(GameTime time,ref WeatherManager.CurrentWeather cw);
        public abstract void OneSecondUpdateTicked(OneSecondUpdateTickedEventArgs e,ref WeatherManager.CurrentWeather cw);
        public abstract void TenMinuteUpdateTicked(ref WeatherManager.CurrentWeather cw);
    }
}
