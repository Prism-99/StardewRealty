using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces;
using System;


namespace SDV_Realty_Core.Framework.ServiceProviders
{
    internal class PLayerMComms : IPlayerComms
    {
        public override Type[] InitArgs => new Type[] 
        { };

        public override object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == ServiceType)
                return this;

            return null;
        }

        internal override void Initialize(ILoggerService logger, object[] args)
        {
            this.logger=logger;
        }
        internal override void SendPlayerMail(string mailId, string expansionName, bool addToReadList)
        {
            SendPlayerMail(mailId, expansionName, addToReadList, false);
        }
        internal override void SendPlayerMail(string mailId, string expansionName, bool addToReadList, bool sendToday)
        {
            if (sendToday)
            {
                if (!Game1.player.mailbox.Contains(mailId))
                {
                    Game1.player.mailbox.Add(mailId);
                }
            }
            else
            {
                if (!Game1.player.mailForTomorrow.Contains(mailId))
                {
                    Game1.player.mailForTomorrow.Add(mailId);
                }
            }
           
        }

    }
}
