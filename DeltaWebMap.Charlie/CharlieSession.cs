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

        public Dictionary<string, UAssetBlueprint> dinoEntries;

        public const string DEFAULT_MOD_ID = "ARK_BASE_GAME";

        public CharlieSession(UEInstall install, CharlieConfig config)
        {
            this.install = install;
            this.config = config;

            this.persist = new CharliePersist(config);
            this.persist.Load();

            this.assetManager = new AssetManager(config, persist, new AssetManagerTransportFirebase());
            this.assetManager.transport.StartSession(config);
        }

        public void Run()
        {
            //Ppen the Primal Game Data
            UAssetBlueprint pgd = install.OpenBlueprint(install.GetFileFromGamePath("/Game/PrimalEarth/CoreBlueprints/PrimalGameData_BP.uasset").GetFilename());

            //Open all dino entries
            dinoEntries = GetDinoEntries(pgd);

            //Seek the files
            AssetSeeker s = new AssetSeeker(install, config.exclude_regex);
            var files = s.SeekAssets(persist);

            //Create queues
            List<WriteModel<DbArkEntry<DinosaurEntry>>> queueDinos = new List<WriteModel<DbArkEntry<DinosaurEntry>>>();
            List<WriteModel<DbArkEntry<ItemEntry>>> queueItems = new List<WriteModel<DbArkEntry<ItemEntry>>>();

            //Now run each file
            foreach (var f in files)
            {
                //Open blueprint
                UAssetBlueprint bp;
                Console.ForegroundColor = ConsoleColor.Red;
                try
                {
                    bp = install.OpenBlueprint(f.Key);
                }
                catch (FailedToFindDefaultsException)
                {
                    Console.WriteLine("FAILED TO FIND DEFAULTS");
                    continue;
                }
                Console.ForegroundColor = ConsoleColor.Yellow;

                //Decode data and write it
                try
                {
                    //Insert in database
                    if (f.Value == DiscoveredFileType.Dino)
                    {
                        DinosaurEntry entry = DinoConverter.ConvertDino(this, bp);
                        if (entry == null)
                            continue;
                        QueueEntryDb(queueDinos, entry, entry.classname, DEFAULT_MOD_ID);
                    }
                } catch (Exception ex)
                {
                    Console.WriteLine("Error while parsing: " + ex.Message);
                }

                Console.ForegroundColor = ConsoleColor.White;
            }

            //Commit changes
            DeltaConnection conn = new DeltaConnection(config.delta_cfg, "CHARLIE-DEPLOY", 1, 1);
            conn.Connect().GetAwaiter().GetResult();
            if (queueDinos.Count > 0)
                conn.arkentries_dinos.BulkWrite(queueDinos);
            if(queueItems.Count > 0)
                conn.arkentries_items.BulkWrite(queueItems);
        }

        public void EndSession()
        {
            assetManager.transport.EndSession();
        }

        private void QueueEntryDb<T>(List<WriteModel<DbArkEntry<T>>> queue, T payload, string classname, string mod)
        {
            //Create
            DbArkEntry<T> entry = new DbArkEntry<T>
            {
                classname = classname,
                mod = mod,
                time = DateTime.UtcNow,
                data = payload
            };

            //Create filter for updating this
            var filterBuilder = Builders<DbArkEntry<T>>.Filter;
            var filter = filterBuilder.Eq("classname", classname) & filterBuilder.Eq("mod", mod);

            //Now, add (or insert) this into the database
            var a = new ReplaceOneModel<DbArkEntry<T>>(filter, entry);
            a.IsUpsert = true;
            queue.Add(a);
        }

        private Dictionary<string, UAssetBlueprint> GetDinoEntries(UAssetBlueprint pgd)
        {
            //Get the array in which data is stored in 
            ArrayProperty entriesArray = pgd.defaults.GetPropertyByName<ArrayProperty>("DinoEntries");

            //Read all
            Dictionary<string, UAssetBlueprint> map = new Dictionary<string, UAssetBlueprint>();
            foreach(var e in entriesArray.properties)
            {
                ObjectProperty prop = (ObjectProperty)e;
                UAssetBlueprint bp = prop.GetReferencedBlueprintAsset();
                string tag = bp.defaults.GetPropertyByName<NameProperty>("DinoNameTag").valueName;
                if(!map.ContainsKey(tag))
                    map.Add(tag, bp);
            }
            return map;
        }
    }
}
