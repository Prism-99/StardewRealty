using SDV_Realty_Core.Framework.CustomEntities.Movies;
using System;
using System.Collections.Generic;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.CustomEntities
{
    internal abstract class ICustomMovieService:IService
{
        public override Type ServiceType => typeof(ICustomMovieService);
        public abstract Dictionary<string, CustomMovieData> Movies { get; }
        public abstract Dictionary<string, object> ExternalReferences { get; }
        public abstract void LoadDefinitions();
    }
}
