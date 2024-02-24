using System;
using System.Xml;
using System.Xml.Serialization;
using Prism99_Core.Utilities;
//using SDictOld = Prism99_Core.Objects.SerializableDictionary<string, SDV_Realty_Core.Framework.Saves.OldExpansion>;
using SDict = Prism99_Core.Objects.SerializableDictionary<string, SDV_Realty_Core.Framework.Expansions.FarmExpansionLocation>;

namespace SDV_Realty_Core.Framework.Saves
{
    internal class Save_V1_5 : ISDRSave
    {
        //public static XmlSerializer OldlocationSerializer = new XmlSerializer(typeof(SDictOld));
        public static XmlSerializer locationSerializer = new XmlSerializer(typeof(SDict));
        public override string Version => "0.0.0";
        public Save_V1_5(SDVLogger sdvLogger, IModHelper modHelper)
        {
            logger = sdvLogger;
            helper = modHelper;
        }
        public override bool LoadSaveFile(string saveFilePath)
        {
            //SDictOld oLoaded = null;
            //string saveFilePath = Path.Combine(helper?.DirectoryPath ?? "", "pslocationdata", $"{Constants.SaveFolderName}.xml");
            try
            {
                using (var reader = XmlReader.Create(saveFilePath))
                {
                    //oLoaded = (SDictOld)OldlocationSerializer.Deserialize(reader);
                    //if (oLoaded != null)
                    //{
                    //    //FarmExpansions = oLoaded;
                    //    //ProcessSaveDate(FarmExpansions);
                    //    return true;
                    //}
                }
            }
            catch (Exception ex)
            {
            }
            return false;
        }

        public override bool SaveFile(string filename)
        {
            throw new NotImplementedException();
        }
    }
}
