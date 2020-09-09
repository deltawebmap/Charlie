using DeltaWebMap.Charlie.Framework.UE;
using DeltaWebMap.Charlie.Framework.UE.Assets.UAssetTypes;
using DeltaWebMap.Charlie.Framework.UE.PropertyReader.Properties;
using LibDeltaSystem.Db.ArkEntries;
using LibDeltaSystem.Entities.ArkEntries.Dinosaur;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeltaWebMap.Charlie.Framework
{
    /// <summary>
    /// Package containing ARK content. Holds a PrimalGameData
    /// </summary>
    public class ArkPackage
    {
        public readonly CharlieSession session;
        private UAssetBlueprint primalGameData;
        private string primalGameDataPath;
        private string primalGameDataKey;

        public Dictionary<string, UAssetBlueprint> dinoEntries;
        public List<WriteModel<DbArkEntry<DinosaurEntry>>> queueDinos;
        public List<WriteModel<DbArkEntry<ItemEntry>>> queueItems;
        public DbPrimalPackage packageDinos;
        public DbPrimalPackage packageItems;

        public long modId;
        public string title;

        public ArkPackage(CharlieSession session, long modId, string title, string primalGameDataPath, string primalGameDataKey)
        {
            this.session = session;
            this.modId = modId;
            this.title = title;
            this.primalGameDataPath = primalGameDataPath;
            this.primalGameDataKey = primalGameDataKey;

            //Create queues
            queueDinos = new List<WriteModel<DbArkEntry<DinosaurEntry>>>();
            queueItems = new List<WriteModel<DbArkEntry<ItemEntry>>>();

            //Get packages
            packageDinos = GetModPackage("SPECIES");
            packageItems = GetModPackage("ITEMS");
        }

        public virtual void InitPackage()
        {
            //Open the Primal Game Data
            primalGameData = session.install.OpenBlueprint(session.install.GetFileFromGamePath(primalGameDataPath).GetFilename());

            //Open all dino entries
            dinoEntries = GetDinoEntries(primalGameData, primalGameDataKey);
        }

        /// <summary>
        /// Returns true if a game path is located inside of a package
        /// </summary>
        /// <param name="gamePath"></param>
        /// <returns></returns>
        public virtual bool IsFileBelongingToPackage(string gamePath)
        {
            //Return false by default, as this will be chosen if nothing else is picked regardless
            return false;
        }

        public void CommitChanges()
        {
            //Commit changes
            if (queueDinos.Count > 0)
                Program.db.arkentries_dinos.BulkWrite(queueDinos);
            if (queueItems.Count > 0)
                Program.db.arkentries_items.BulkWrite(queueItems);

            //Commit packages
            packageDinos.UpdateModifiedTimeAsync(Program.db).GetAwaiter().GetResult();
            packageItems.UpdateModifiedTimeAsync(Program.db).GetAwaiter().GetResult();
        }

        private DbPrimalPackage GetModPackage(string packageType)
        {
            //Try to get existing packages
            var package = Program.db.GetPrimalPackageByModAsync(modId, packageType).GetAwaiter().GetResult();
            if (package != null)
                return package;

            //Create new package
            package = new DbPrimalPackage
            {
                last_updated = DateTime.UtcNow.Ticks,
                mod_id = modId,
                name = Guid.NewGuid().ToString(),
                package_type = packageType,
                display_name = title
            };

            //Insert
            Program.db.arkentries_primal_packages.InsertOne(package);
            session.Log("GetModPackage", $"Package '{package.display_name}' ({package.name}) created for {modId}:{packageType}", ConsoleColor.Green);

            return package;
        }

        private Dictionary<string, UAssetBlueprint> GetDinoEntries(UAssetBlueprint pgd, string key = "DinoEntries")
        {
            //Get the array in which data is stored in 
            ArrayProperty entriesArray = pgd.defaults.GetPropertyByName<ArrayProperty>(key);

            //Read all
            Dictionary<string, UAssetBlueprint> map = new Dictionary<string, UAssetBlueprint>();
            if (entriesArray != null)
            {
                foreach (var e in entriesArray.properties)
                {
                    ObjectProperty prop = (ObjectProperty)e;
                    UAssetBlueprint bp = prop.GetReferencedBlueprintAsset();
                    string tag = bp.defaults.GetPropertyByName<NameProperty>("DinoNameTag").valueName;
                    if (!map.ContainsKey(tag))
                        map.Add(tag, bp);
                }
            } else
            {
                //The property wasn't found. This is most likely just a mod without dino entries. Maybe we should raise an error?
            }
            return map;
        }
    }
}
