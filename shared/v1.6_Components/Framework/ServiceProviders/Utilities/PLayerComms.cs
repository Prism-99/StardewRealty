using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces;
using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;


namespace SDV_Realty_Core.Framework.ServiceProviders.Utilities
{
    /// <summary>
    /// Handles sending mail to the player
    /// </summary>
    internal class PLayerMComms : IPlayerComms
    {
        private IModDataService _ModDataService;

        public override Type[] InitArgs => new Type[]
        {typeof(IModDataService) };

        public override object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == ServiceType)
                return this;

            return null;
        }

        internal override void Initialize(ILoggerService logger, object[] args)
        {
            this.logger = logger;
            _ModDataService = (IModDataService)args[0];
        }
        /// <summary>
        /// Add mail to the player's mailbox
        /// </summary>
        /// <param name="mailId">Id of the message to be sent</param>
        /// <param name="sendToday">Put the message into today's mail</param>
        /// <param name="recordSend">Check to see Id has been previously sent</param>
        /// <param name="addToReadList">Put the message into the player's read box</param>
        internal override void SendPlayerMail(string mailId, bool sendToday = false, bool recordSend = false, bool addToReadList = false)
        {
            if (!recordSend || !_ModDataService.ModState.PurchaseLettersSent.Contains(mailId))
            {
                if (sendToday)
                {
                    if (!Game1.player.mailbox.Contains(mailId))
                    {
                        Game1.player.mailbox.Add(mailId);
                    }
                }
                else if (addToReadList)
                {
                    if (!Game1.player.mailReceived.Contains(mailId))
                    {
                        Game1.player.mailReceived.Add(mailId);
                    }
                    
                }
                else
                {
                    if (!Game1.player.mailForTomorrow.Contains(mailId))
                    {
                        Game1.player.mailForTomorrow.Add(mailId);
                    }
                }
                if (recordSend)
                {
                    _ModDataService.ModState.PurchaseLettersSent.Add(mailId);
                    _ModDataService.SaveModState();
                }
            }
        }
    }
}
