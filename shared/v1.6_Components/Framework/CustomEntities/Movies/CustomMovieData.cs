using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using SDV_Realty_Core.ContentPackFramework.ContentPacks;


namespace SDV_Realty_Core.Framework.CustomEntities.Movies
{
    internal class CustomMovieData : ISDRContentPack
    {
        public string MovieId { get; set; }
        public SDRMovieData MovieData { get; set; }
        public override string PackFileName => "moviedata.json";

        public override ISDRContentPack ReadContentPack(string fileName)
        {
            string fileContent = File.ReadAllText(fileName);
            CustomMovieData newMovie = JsonConvert.DeserializeObject<CustomMovieData>(fileContent);
            newMovie.ModPath = Path.GetDirectoryName(fileName);

            return newMovie;
        }
    }
}
