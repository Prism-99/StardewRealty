using Newtonsoft.Json;
using SDV_Realty_Core.ContentPackFramework.Utilities;
using SDV_Realty_Core.Framework.CustomEntities.Movies;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using SDV_Realty_Core.Framework.ServiceInterfaces.CustomEntities;
using StardewValley.GameData.Movies;
using System;
using System.Collections.Generic;
using System.IO;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;

namespace SDV_Realty_Core.Framework.ServiceProviders.CustomEntities
{
    internal class CustomMovieService : ICustomMovieService
    {
        private Dictionary<string, CustomMovieData> _movies = new();
        private IModHelperService _modHelperService;
        private Dictionary<string, object> _externalReferences = new();
        public override Type[] InitArgs => new Type[]
        {
            typeof(IModHelperService)
        };

        public override Dictionary<string, CustomMovieData> Movies => _movies;
        public override Dictionary<string, object> ExternalReferences => _externalReferences;
        public override object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == ServiceType)
                return this;

            return null;
        }

        internal override void Initialize(ILoggerService logger, object[] args)
        {
            this.logger = logger;
            _modHelperService = (IModHelperService)args[0];
            _movies = new Dictionary<string, CustomMovieData>();
        }
        public override void LoadDefinitions()
        {
            try
            {
                string buildingRoot = Path.Combine(_modHelperService.DirectoryPath, "data", "assets", "movies");
                if (Directory.Exists(buildingRoot))
                {
                    string[] arDefinitions = Directory.GetFiles(buildingRoot, "moviedata.json", SearchOption.AllDirectories);
                    logger.Log($"Found {arDefinitions.Length} custom movie definitions.", LogLevel.Debug);

                    foreach (string defin in arDefinitions)
                    {
                        try
                        {
                            string fileContent = File.ReadAllText(defin);
                            CustomMovieData newMovie = JsonConvert.DeserializeObject<CustomMovieData>(fileContent);
                            newMovie.ModPath = Path.GetDirectoryName(defin);
                            newMovie.translations = _modHelperService.Translation;
                            //nBuilding.LoadExternalReferences();
                            AddMovieDefinition(newMovie.MovieId, newMovie);
                        }
                        catch (Exception ex)
                        {
                            logger.Log($"Error loading movie defintion: {ex}", LogLevel.Error);
                        }
                    }
                }
                else
                {
                    logger.Log($"Missing custom movie directory '{buildingRoot}'", LogLevel.Warn);
                }
                // AddTestMovie(translations);
            }
            catch (Exception ex) { }

        }
        public void AddMovieDefinition(string movieKey, CustomMovieData movie)
        {
            if (!string.IsNullOrEmpty(movie.MovieData.Texture))
            {
                string moviePath = $"SDR{FEConstants.AssetDelimiter}movies{FEConstants.AssetDelimiter}{movieKey}{FEConstants.AssetDelimiter}{movie.MovieData.Texture}";
                _externalReferences.Add(moviePath, Path.Combine(movie.ModPath, movie.MovieData.Texture));
                movie.MovieData.Texture = moviePath;
            }

            Movies.Add(movieKey, movie);

        }
        private void AddTestMovie()
        {
            AddMovieDefinition("test", new CustomMovieData
            {
                ModPath = Path.Combine(_modHelperService.DirectoryPath, "data", "assets", "movies", "test"),
                translations = _modHelperService.Translation,
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

    }
}
