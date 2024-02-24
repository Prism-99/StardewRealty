using SDV_Realty_Core.Framework.CustomEntities.Movies;
using SDV_Realty_Core.Framework.ServiceInterfaces.CustomEntities;
using StardewModdingAPI.Events;
using StardewValley.GameData.Movies;
using System;
using System.Collections.Generic;


namespace SDV_Realty_Core.Framework.DataProviders
{
    internal class MoviesDataProvider : IGameDataProvider
    {
        public override string Name => "Data/Movies";
        private ICustomMovieService _customMovieService;
        public MoviesDataProvider(ICustomMovieService customMovieService)
        {
            _customMovieService = customMovieService;
        }
        public override void CheckForActivations()
        {

        }

        public override void HandleEdit(AssetRequestedEventArgs e)
        {
            e.Edit(asset =>
            {
                var movieData =(List<MovieData>) asset.Data;

                logger.Log($"movieData is null {movieData==null}", StardewModdingAPI.LogLevel.Debug);
                foreach (var movie in _customMovieService.Movies)
                {
                    try
                    {
                        movieData.Add( GetMovieData(movie.Value.MovieData));
                    }
                    catch(Exception ex) {
                        logger.Log($"Error load movie data: {ex}", StardewModdingAPI.LogLevel.Debug);
                    }
                }
            });
        }
        private MovieData GetMovieData(SDRMovieData source)
        {
            return new MovieData
            {
                CranePrizes = source.CranePrizes,
                Scenes = source.Scenes,
                Description = source.Description,
                Id = source.Id,
                Seasons = source.Seasons,
                SheetIndex = source.SheetIndex,
                Texture = source.Texture,
                Title = source.Title,
                Tags = source.Tags
            };
        }
        public override void OnGameLaunched()
        {

        }
    }
}
