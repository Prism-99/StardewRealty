using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using System.IO;
using SDict = Prism99_Core.Objects.SerializableDictionary<string, SDV_Realty_Core.Framework.Expansions.FarmExpansionLocation>;
using SubDict= Prism99_Core.Objects.SerializableDictionary<string, StardewValley.GameLocation>;
namespace SDV_Realty_Core.Framework.Saves
{
    public abstract class ISDRSave
    {
      internal  IModHelper helper;
        internal ILoggerService logger;
        public abstract string Version { get; }
        public abstract bool LoadSaveFile(string filename, bool loadOnly = false);
        public abstract bool SaveFile(string filename);
        public virtual SDict FarmExpansions { get; set; }
        public virtual SubDict SubLocations { get; set; }
        public virtual string GetSaveFilename()
        {
           string saveFilename = Path.Combine(helper?.DirectoryPath ?? "", "pslocationdata", $"{Constants.SaveFolderName}.xml");
            string saveDirectory = Path.GetDirectoryName(saveFilename);

            if (saveDirectory == null)
                return null;

            if (!Directory.Exists(saveDirectory))
                Directory.CreateDirectory(saveDirectory);

            return saveFilename;
        }
        public virtual bool BackupExists(out string backupFilename)
        {
            backupFilename = Path.Combine(helper?.DirectoryPath ?? "", "pslocationdata", $"{Constants.SaveFolderName}.txt");

            return File.Exists(backupFilename);

        }
    }
}
