using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DeltaWebMap.Charlie.Framework
{
    /// <summary>
    /// Contains data for a mod
    /// </summary>
    public class ArkModPackage : ArkPackage
    {
        public ArkModPackage(CharlieSession session, long modId, string modName, string primalGameDataPath, string primalGameDataKey) : base(session, modId, modName, primalGameDataPath, primalGameDataKey)
        {
            this.modName = modName;
        }

        public string modName;
        public string folderPath;
        public string folderName;
        public string installPath; //Only if we have a PrimalGameData
        public Dictionary<string, string> customProperties;

        public override void InitPackage()
        {
            //Add remap
            if(installPath != "")
                session.install.AddRemap("/Game/Mods/" + installPath, "/Game/Mods/" + modId.ToString());

            //Run base
            base.InitPackage();
        }

        public override bool IsFileBelongingToPackage(string gamePath)
        {
            return gamePath.StartsWith("/Game/Mods/" + installPath);
        }

        public static ArkModPackage GetArkModPackageFromMod(CharlieSession session, string modFile)
        {
            //Open the mod info file (<id>.mod)
            ArkModPackage package;
            using (FileStream fs = new FileStream(modFile, FileMode.Open))
            using(IOMemoryStream reader = new IOMemoryStream(fs, true))
            {
                //Read the mod ID. This is an Int64 at the beginning of the file
                long modId = reader.ReadLong();

                //Read mod name
                string modName = reader.ReadUEString();

                //Read folder path
                string folderPath = reader.ReadUEString();

                //Read unknown int
                int unknown1 = reader.ReadInt();

                //Read folder name
                string folderName = reader.ReadUEString();

                //Skip 13 bytes. I don't know what's in this region.
                fs.Position += 9;

                //Read the length of the dictonary
                int dictLength = reader.ReadInt();

                //Read the dictonary. This is just a bunch of strings
                Dictionary<string, string> props = new Dictionary<string, string>(dictLength);
                for(int i = 0; i<dictLength; i+=1)
                {
                    string key = reader.ReadUEString();
                    string value = reader.ReadUEString();
                    props.Add(key, value);
                }

                //Determine the PrimalGameData path, if any
                string primalGameData = "";
                if (props.ContainsKey("PrimalGameData"))
                    primalGameData = props["PrimalGameData"];

                //Find the installPath. This is a hack.
                //The idea is that we take the install location from the PrimalGameData path. That should always start with /Game/Mods/[installPath]/.
                string installPath = "";
                if (primalGameData != "")
                {
                    //Validate
                    if (!primalGameData.StartsWith("/Game/Mods/"))
                        throw new Exception("Failed to find installPath.");

                    //Trim
                    installPath = primalGameData.Substring("/Game/Mods/".Length);

                    //Make this only the first namespace
                    installPath = installPath.Substring(0, installPath.IndexOf('/'));
                }

                //Create a new mod package
                package = new ArkModPackage(session, modId, modName, primalGameData + ".uasset", "AdditionalDinoEntries");
                package.folderPath = folderPath;
                package.folderName = folderName;
                package.installPath = installPath;
                package.customProperties = props;
            }

            return package;
        }
    }
}
