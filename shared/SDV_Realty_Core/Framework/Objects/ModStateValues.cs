using System.Collections.Generic;


namespace SDV_Realty_Core.Framework.Objects
{
    /// <summary>
    /// Store any required mod state values
    /// 
    /// stored with game save
    /// 
    /// </summary>
    internal class ModStateValues
    {
        // junimo fee letter has been sent
        public bool JunimoLetterSent { get; set; }
        // contents of junimo fee letter
        public string JunimoLetter { get; set; } = string.Empty;
        // list of expansion purchased letters sent
        public List<string> PurchaseLettersSent { get; set; }=new List<string>();
        // list of offer letters sent
        public Dictionary<string, string> MailToBeRead = new Dictionary<string, string> { };

    }
}
