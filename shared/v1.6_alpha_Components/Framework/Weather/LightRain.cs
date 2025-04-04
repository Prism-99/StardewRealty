﻿using Microsoft.Xna.Framework;
using StardewValley.Locations;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;

namespace SDV_Realty_Core.Framework.Weather
{
    internal class LightRain : IWeatherCondition
    {
        private int DropCount = 20;

        public void Draw(ref WeatherManager.CurrentWeather cw)
        {
            Game1.spriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, SamplerState.PointClamp);

            for (int i = 0; i < cw.IntParam1; i++)
            {
                Game1.spriteBatch.Draw(Game1.rainTexture, Game1.rainDrops[i].position, Game1.getSourceRectForStandardTileSheet(Game1.rainTexture, Game1.rainDrops[i].frame), Color.White);
            }
            Game1.spriteBatch.End();
        }
        public string Name()
        {
            return "prism99.advize.stardewrealty.light_rain";
        }
        public string DisplayName()
        {
            return "Light Rain";
        }
        public void UpdateTicked(ref WeatherManager.CurrentWeather cw)
        {
        }
        public void OneSecondUpdateTicked(OneSecondUpdateTickedEventArgs e,ref WeatherManager.CurrentWeather cw)
        {
            switch (cw.PreviousWeather)
            {
                case "Sunny":
                     if (cw.IntParam1 < DropCount)
                        cw.IntParam1++;
                    break;
                case "Drizzle":
                case "Rain":
                    if (cw.IntParam1 > DropCount)
                        cw.IntParam1--;
                    break;
                default:
                    if (cw.IntParam1 < DropCount)
                        cw.IntParam1++;
                    break;
            }
        }

        public void StartCycle(ref WeatherManager.CurrentWeather cw)
        {
            switch (cw.PreviousWeather)
            {
                case "Sunny":
                    cw.IntParam1 = 1;
                    break;
                case "Rain":
                    cw.IntParam1 = 70;
                    break;
                case "Drizzle":
                    cw.IntParam1 = 40;
                    break;
                default:
                    cw.IntParam1 = 1;
                    break;
            }
        }

        public void TenMinuteUpdateTicked(ref WeatherManager.CurrentWeather cw)
        {

        }

        public void Update(GameTime time,ref WeatherManager.CurrentWeather cw)
        {

            for (int i = 0; i < cw.IntParam1; i++)
            {
                if (Game1.rainDrops[i].frame == 0)
                {
                    Game1.rainDrops[i].accumulator += time.ElapsedGameTime.Milliseconds;
                    if (Game1.rainDrops[i].accumulator < 70)
                    {
                        continue;
                    }
                    Game1.rainDrops[i].position += new Vector2(-16 + i * 8 / cw.IntParam1, 32 - i * 8 / cw.IntParam1);
                    Game1.rainDrops[i].accumulator = 0;
                    if (Game1.random.NextDouble() < 0.1)
                    {
                        Game1.rainDrops[i].frame++;
                    }
                    if (Game1.currentLocation is IslandNorth || Game1.currentLocation is Caldera)
                    {
                        Point p = new Point((int)(Game1.rainDrops[i].position.X + (float)Game1.viewport.X) / 64, (int)(Game1.rainDrops[i].position.Y + (float)Game1.viewport.Y) / 64);
                        p.Y--;
                        if (Game1.currentLocation.isTileOnMap(p.X, p.Y) && Game1.currentLocation.getTileIndexAt(p, "Back") == -1 && Game1.currentLocation.getTileIndexAt(p, "Buildings") == -1)
                        {
                            Game1.rainDrops[i].frame = 0;
                        }
                    }
                    if (Game1.rainDrops[i].position.Y > (float)(Game1.viewport.Height + 64))
                    {
                        Game1.rainDrops[i].position.Y = -64f;
                    }
                    continue;
                }
                Game1.rainDrops[i].accumulator += time.ElapsedGameTime.Milliseconds;
                if (Game1.rainDrops[i].accumulator > 70)
                {
                    Game1.rainDrops[i].frame = (Game1.rainDrops[i].frame + 1) % 4;
                    Game1.rainDrops[i].accumulator = 0;
                    if (Game1.rainDrops[i].frame == 0)
                    {
                        Game1.rainDrops[i].position = new Vector2(Game1.random.Next(Game1.viewport.Width), Game1.random.Next(Game1.viewport.Height));
                    }
                }
            }

        }
    }
}
