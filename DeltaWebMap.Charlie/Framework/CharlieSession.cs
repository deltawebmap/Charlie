using DeltaWebMap.Charlie.Converters;
using DeltaWebMap.Charlie.Framework;
using DeltaWebMap.Charlie.Framework.Exceptions;
using DeltaWebMap.Charlie.Framework.Managers;
using DeltaWebMap.Charlie.Framework.Managers.AssetManagerTransports;
using DeltaWebMap.Charlie.Framework.UE;
using DeltaWebMap.Charlie.Framework.UE.AssetDiscoverEngine;
using DeltaWebMap.Charlie.Framework.UE.Assets.UAssetTypes;
using DeltaWebMap.Charlie.Framework.UE.PropertyReader.Properties;
using LibDeltaSystem;
using LibDeltaSystem.Db.ArkEntries;
using LibDeltaSystem.Entities.ArkEntries.Dinosaur;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeltaWebMap.Charlie
{
    public class CharlieSession
    {
        public UEInstall install;
        public CharlieConfig config;
        public CharliePersist persist;
        public AssetManager assetManager;
        public ObjectId id;
        public DateTime beginTime;

        public int assetsUploaded;
        public int entriesUpdated;
        public int entriesScanned;

        public const string DEFAULT_MOD_ID = "ARK_BASE_GAME";

        public CharlieSession(UEInstall install, CharlieConfig config)
        {
            this.install = install;
            this.config = config;
            this.id = ObjectId.GenerateNewId();
            this.beginTime = DateTime.UtcNow;

            this.persist = new CharliePersist(config);
            this.persist.Load();

            this.assetManager = new AssetManager(config, this, new AssetManagerTransportFirebase());
        }

        public void Run()
        {
            //Ppen the Primal Game Data
            UAssetBlueprint pgd = install.OpenBlueprint(install.GetFileFromGamePath("/Game/PrimalEarth/CoreBlueprints/PrimalGameData_BP.uasset").GetFilename());

            //Open all dino entries
            Dictionary<string, UAssetBlueprint> dinoEntries = GetDinoEntries(pgd);

            //Seek the files
            AssetSeeker s = new AssetSeeker(install, config.exclude_regex);
            var files = s.SeekAssets(persist);

            //Create queues
            List<WriteModel<DbArkEntry<DinosaurEntry>>> queueDinos = new List<WriteModel<DbArkEntry<DinosaurEntry>>>();
            List<WriteModel<DbArkEntry<ItemEntry>>> queueItems = new List<WriteModel<DbArkEntry<ItemEntry>>>();

            //Get packages
            var dinoPackage = GetModPackage(0, "SPECIES");
            var itemPackage = GetModPackage(0, "ITEMS");

            //Now run each file
            foreach (var f in files)
            {
                //Open blueprint
                UAssetBlueprint bp;
                try
                {
                    entriesScanned++;
                    bp = install.OpenBlueprint(f.Key);
                }
                catch (FailedToFindDefaultsException)
                {
                    Log("ConvertItem", $"[{f.Key}-{f.Value}] Failed to find blueprint results!", ConsoleColor.Magenta);
                    continue;
                }
                catch
                {
                    Log("ConvertItem", $"[{f.Key}-{f.Value}] Failed to open blueprint!", ConsoleColor.Magenta);
                    continue;
                }

                //Read the defaults and determine the true type
                DiscoveredFileType trueType = DiscoveredFileType.None;
                if (bp.defaults.HasProperty("DescriptiveName") && bp.defaults.HasProperty("DinoNameTag"))
                    trueType = DiscoveredFileType.Dino;
                if (bp.defaults.HasProperty("DescriptiveNameBase") && bp.defaults.HasProperty("ItemDescription"))
                    trueType = DiscoveredFileType.Item;

                //Check if the true type is valid
                if (f.Value != DiscoveredFileType.Structure && (int)trueType <= 0)
                {
                    //This failed to identify as a valid type!
                    Log("ConvertItem", $"[{f.Key}-{f.Value}] Failed to identify true type of the object!", ConsoleColor.Magenta);
                    continue;
                }

                //Decode data and write it
                string classname = f.Key;
                try
                {
                    //Insert in database
                    if (trueType == DiscoveredFileType.Dino)
                    {
                        DinosaurEntry entry = DinoConverter.ConvertDino(this, bp, dinoEntries);
                        if (entry == null)
                            continue;
                        QueueEntryDb(queueDinos, entry, entry.classname, dinoPackage);
                        classname = entry.classname;
                    }
                    else if (trueType == DiscoveredFileType.Item)
                    {
                        ItemEntry entry = ItemConverter.ConvertItem(this, bp);
                        if (entry == null)
                            continue;
                        QueueEntryDb(queueItems, entry, entry.classname, itemPackage);
                        classname = entry.classname;
                    } else if(f.Value == DiscoveredFileType.Structure)
                    {
                        //Ignore structures
                        continue;
                    } else
                    {
                        throw new Exception("Unknown true type!");
                    }

                    //Add
                    Log("ConvertItem", $"[{classname}-{f.Value}] Converted as {trueType.ToString().ToUpper()}.", ConsoleColor.Green);
                    entriesUpdated++;
                }
                catch (Exception ex)
                {
                    Log("ConvertItem", $"[{classname}-{f.Value}] (True type {trueType}) Entity conversion failed: " + ex.Message + ex.StackTrace, ConsoleColor.Red);
                }
            }

            //Commit changes
            if (queueDinos.Count > 0)
                Program.db.arkentries_dinos.BulkWrite(queueDinos);
            if (queueItems.Count > 0)
                Program.db.arkentries_items.BulkWrite(queueItems);

            //Commit packages
            dinoPackage.UpdateModifiedTimeAsync(Program.db).GetAwaiter().GetResult();
            itemPackage.UpdateModifiedTimeAsync(Program.db).GetAwaiter().GetResult();
        }

        private static void QueueEntryDb<T>(List<WriteModel<DbArkEntry<T>>> queue, T payload, string classname, DbPrimalPackage package)
        {
            //Create
            DbArkEntry<T> entry = new DbArkEntry<T>
            {
                classname = classname,
                time = DateTime.UtcNow.Ticks,
                data = payload,
                package_name = package.name
            };

            //Create filter for updating this
            var filterBuilder = Builders<DbArkEntry<T>>.Filter;
            var filter = filterBuilder.Eq("classname", classname) & filterBuilder.Eq("package_name", package.name);

            //Now, add (or insert) this into the database
            var a = new ReplaceOneModel<DbArkEntry<T>>(filter, entry);
            a.IsUpsert = true;
            queue.Add(a);
        }

        private static Dictionary<string, UAssetBlueprint> GetDinoEntries(UAssetBlueprint pgd)
        {
            //Get the array in which data is stored in 
            ArrayProperty entriesArray = pgd.defaults.GetPropertyByName<ArrayProperty>("DinoEntries");

            //Read all
            Dictionary<string, UAssetBlueprint> map = new Dictionary<string, UAssetBlueprint>();
            foreach (var e in entriesArray.properties)
            {
                ObjectProperty prop = (ObjectProperty)e;
                UAssetBlueprint bp = prop.GetReferencedBlueprintAsset();
                string tag = bp.defaults.GetPropertyByName<NameProperty>("DinoNameTag").valueName;
                if (!map.ContainsKey(tag))
                    map.Add(tag, bp);
            }
            return map;
        }

        public void EndSession()
        {
            assetManager.FinalizeItems();
        }

        public DbPrimalPackage GetModPackage(long modId, string packageType)
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
                package_type = packageType
            };

            //Insert
            Program.db.arkentries_primal_packages.InsertOne(package);
            Log("GetModPackage", $"Package {package.name} created for {modId}:{packageType}", ConsoleColor.Green);

            return package;
        }

        public void Log(string topic, string msg, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"[{topic}] {msg}");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
