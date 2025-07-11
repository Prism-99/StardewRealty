using Microsoft.Xna.Framework.Content;
using StardewValley;
using StardewValley.GameData.Movies;
using System;
using System.Collections.Generic;
using System.Text;

namespace SDV_Realty_Core.Framework.CustomEntities.Movies
{
    internal class SDRMovieData
{
        //
        // Summary:
        //     A key which uniquely identifies this movie. This should only contain alphanumeric/underscore/dot
        //     characters. For custom movies, this should be prefixed with your mod ID like
        //     Example.ModId_MovieName.
        [ContentSerializer(Optional = true)]
        public string Id;

        //
        // Summary:
        //     The seasons when the movie plays, or none to allow any season.
        [ContentSerializer(Optional = true)]
        public List<Season> Seasons;

        //
        // Summary:
        //     If set, the movie is available when {year} % StardewValley.GameData.Movies.MovieData.YearModulus
        //     == StardewValley.GameData.Movies.MovieData.YearRemainder (where {year} is the
        //     number of years since the movie theater was built and {remainder} defaults to
        //     zero). For example, a modulus of 2 with remainder 1 is shown in the second year
        //     and every other year thereafter.
        [ContentSerializer(Optional = true)]
        public int? YearModulus;

        [ContentSerializer(Optional = true)]
        public int? YearRemainder;

        //
        // Summary:
        //     The asset name for the movie poster and screen images, or null to use LooseSprites\Movies.
        //
        // Remarks:
        //     This must be a spritesheet with one 490×128 pixel row per movie. A 13×19 area
        //     in the top-left corner of the row should contain the movie poster. With a 16-pixel
        //     offset from the left edge, there should be two rows of five 90×61 pixel movie
        //     screen images, with a six-pixel gap between each image. (The movie doesn't need
        //     to use all of the image slots.)
        [ContentSerializer(Optional = true)]
        public string Texture;

        //
        // Summary:
        //     The sprite index within the StardewValley.GameData.Movies.MovieData.Texture for
        //     this movie poster and screen images.
        public int SheetIndex;

        //
        // Summary:
        //     A tokenizable string for the translated movie title.
        public string Title;

        //
        // Summary:
        //     A tokenizable string for the translated movie description, shown when interacting
        //     with the movie poster.
        public string Description;

        //
        // Summary:
        //     A list of tags which describe the genre or other metadata, which can be matched
        //     by StardewValley.GameData.Movies.MovieReaction.Tag.
        [ContentSerializer(Optional = true)]
        public List<string> Tags;

        //
        // Summary:
        //     The prizes that can be grabbed in the crane game while this movie is playing
        //     (in addition to the default items).
        [ContentSerializer(Optional = true)]
        public List<MovieCranePrizeData> CranePrizes = new List<MovieCranePrizeData>();

        //
        // Summary:
        //     The prize rarity lists whose default items to clear when this movie is playing,
        //     so they're only taken from StardewValley.GameData.Movies.MovieData.CranePrizes.
        [ContentSerializer(Optional = true)]
        public List<int> ClearDefaultCranePrizeGroups = new List<int>();

        //
        // Summary:
        //     The scenes to show when watching the movie.
        public List<MovieScene> Scenes;

        //
        // Summary:
        //     Custom fields ignored by the base game, for use by mods.
        [ContentSerializer(Optional = true)]
        public Dictionary<string, string> CustomFields = new Dictionary<string, string>();
    }
}
