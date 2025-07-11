using System;
using System.Collections.Generic;
using Prism99_Core.Utilities;
using SDV_Realty_Core.Framework.AssetUtils;
using Newtonsoft.Json;
using System.IO;
using SDV_Realty_Core.ContentPackFramework.Utilities;
using StardewValley.GameData.Movies;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;

namespace SDV_Realty_Core.Framework.CustomEntities.Movies
{
    internal class CustomMovieManager
    {
        private IModHelper helper;
        private SDVLogger logger;
        private IContentManagerService contentManager;
        public Dictionary<string, CustomMovieData> Movies;
        public void Initialize(SDVLogger ologger, IModHelper ohelper, IContentManagerService contentManager)
        {
            helper = ohelper;
            logger = ologger;
            this.contentManager = contentManager;
            Movies = new Dictionary<string, CustomMovieData>();
            //var vanillaMovies = DataLoader.Movies(Game1.content);

            //LoadMovieDefinitions(helper.Translation);
        }
        public void LoadMovieDefinitions(ITranslationHelper translations)
        {
            string buildingRoot = Path.Combine(helper.DirectoryPath, "data", "assets", "movies");
            string[] arDefinitions = Directory.GetFiles(buildingRoot, "moviedata.json", SearchOption.AllDirectories);
            logger.Log($"Found {arDefinitions.Length} custom movie definitions.", LogLevel.Debug);

            foreach (string defin in arDefinitions)
            {
                try
                {
                    string fileContent = File.ReadAllText(defin);
                    CustomMovieData newMovie = JsonConvert.DeserializeObject<CustomMovieData>(fileContent);
                    newMovie.ModPath = Path.GetDirectoryName(defin);
                    newMovie.translations = translations;
                    //nBuilding.LoadExternalReferences();
                    AddMovieDefinition(newMovie.MovieId, newMovie);
                }
                catch (Exception ex)
                {
                    logger.Log($"Error loading movie defintion: {ex}", LogLevel.Error);
                }
            }
            // AddTestMovie(translations);


        }
        private void AddTestMovie(ITranslationHelper translations)
        {
            AddMovieDefinition("test", new CustomMovieData
            {
                ModPath = Path.Combine(helper.DirectoryPath, "data", "assets", "movies", "test"),
                translations = translations,
                MovieData = new SDRMovieData
                {
                    Id = "test",
                    Tags = new List<string> { "sci-fi", "comedy", "family" },
                    Texture = "testmovie.png",
                    Title = "XXX Introduction",
                    Description = "A test movies.",
                    Seasons = new List<Season> { Season.Spring },
                    Scenes = new List<MovieScene>
                      {
                        new MovieScene{
                                    Image= 0,
                        Music= "Lava_Ambient",
                        MessageDelay= 5000,
                        Script= "/pause 4000",
                        Text= "'The movie'",
                        Id= "test_0"
                      },
                         new MovieScene{
                                    Image= 1,
                        Music= "Lava_Ambient",
                        MessageDelay= 5000,
                        Script= "",
                        Text= "'The movie'",
                        Id= "test_1"
                      }

                }
                }
            });

        }
        public void AddMovieDefinition(string movieKey, CustomMovieData movie)
        {
            if (!string.IsNullOrEmpty(movie.MovieData.Texture))
            {
                string moviePath = $"SDR{FEConstants.AssetDelimiter}movies{FEConstants.AssetDelimiter}{movieKey}{FEConstants.AssetDelimiter}{movie.MovieData.Texture}";
                contentManager.AddExternalReference(moviePath, Path.Combine(movie.ModPath, movie.MovieData.Texture));
                movie.MovieData.Texture = moviePath;
            }

            Movies.Add(movieKey, movie);

        }
    }
}
