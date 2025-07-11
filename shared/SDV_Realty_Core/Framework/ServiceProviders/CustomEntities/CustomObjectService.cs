using SDV_Realty_Core.Framework.CustomEntities.Objects;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using SDV_Realty_Core.Framework.ServiceInterfaces.CustomEntities;
using System;
using System.Collections.Generic;


namespace SDV_Realty_Core.Framework.ServiceProviders.CustomEntities
{
    internal class CustomObjectService : ICustomObjectService
    {
        private CustomObjectManager customObjectManager;

        public override Type[] InitArgs => new Type[]
        {
            typeof(IModHelperService)
        };

        public override Dictionary<string, CustomObjectData> objects => customObjectManager.objects;
        public override Dictionary<string, object> ExternalReferences => customObjectManager.ExternalReferences;
        public override object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == ServiceType)
                return this;

            return null;
        }

        internal override void Initialize(ILoggerService logger, object[] args)
        {
            this.logger = logger;
            IModHelperService modHelperService = (IModHelperService)args[0];
 
            customObjectManager = new CustomObjectManager(logger, modHelperService);
        }
        public override void LoadDefinitions()
        {
            customObjectManager.LoadObjectDefinitions();
         }

        public override void AddObjectDefinition(CustomObjectData nObject)
        {
            customObjectManager.AddObjectDefinition(nObject);
        }
    }
}
