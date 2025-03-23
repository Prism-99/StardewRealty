using SDV_Realty_Core.Framework.Weather;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;
using System;


namespace SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics
{
    /// <summary>
    /// Custom weather service
    /// </summary>
    internal abstract class IWeatherManagerService:IService
{
        public WeatherManager WeatherManager;
        public override Type ServiceType => typeof(IWeatherManagerService);
    }
}
