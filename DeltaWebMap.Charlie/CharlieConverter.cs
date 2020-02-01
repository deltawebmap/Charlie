using DeltaWebMap.Charlie.Converters;
using DeltaWebMap.Charlie.Framework.Exceptions;
using DeltaWebMap.Charlie.Framework.UE.AssetDiscoverEngine;
using DeltaWebMap.Charlie.Framework.UE.Assets.UAssetTypes;
using DeltaWebMap.Charlie.Framework.UE.PropertyReader.Properties;
using LibDeltaSystem.Db.ArkEntries;
using LibDeltaSystem.Entities.ArkEntries.Dinosaur;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeltaWebMap.Charlie
{
    public static class CharlieConverter
    {
        public static void Run(CharlieSession session)
        {
            //Ppen the Primal Game Data
            UAssetBlueprint pgd = session.install.OpenBlueprint(session.install.GetFileFromGamePath("/Game/PrimalEarth/CoreBlueprints/PrimalGameData_BP.uasset").GetFilename());

            //Open all dino entries
            Dictionary<string, UAssetBlueprint> dinoEntries = GetDinoEntries(pgd);

            //Seek the files
            AssetSeeker s = new AssetSeeker(session.install, session.config.exclude_regex);
            var files = s.SeekAssets(session.persist);

            //Create queues
            List<WriteModel<DbArkEntry<DinosaurEntry>>> queueDinos = new List<WriteModel<DbArkEntry<DinosaurEntry>>>();
            List<WriteModel<DbArkEntry<ItemEntry>>> queueItems = new List<WriteModel<DbArkEntry<ItemEntry>>>();

            //Now run each file
            foreach (var f in files)
            {
                session.entriesScanned++;

                //Open blueprint
                UAssetBlueprint bp;
                Console.ForegroundColor = ConsoleColor.Red;
                try
                {
                    bp = session.install.OpenBlueprint(f.Key);
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
                        DinosaurEntry entry = DinoConverter.ConvertDino(session, bp, dinoEntries);
                        if (entry == null)
                            continue;
                        QueueEntryDb(queueDinos, entry, entry.classname, CharlieSession.DEFAULT_MOD_ID);
                        session.Log("ConvertItem", $"Converted {entry.classname} as DINO.");
                        session.entriesUpdated++;
                    } else if (f.Value == DiscoveredFileType.Item)
                    {
                        ItemEntry entry = ItemConverter.ConvertItem(session, bp);
                        if (entry == null)
                            continue;
                        QueueEntryDb(queueItems, entry, entry.classname, CharlieSession.DEFAULT_MOD_ID);
                        session.Log("ConvertItem", $"Converted {entry.classname} as ITEM.");
                        session.entriesUpdated++;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error while parsing: " + ex.Message);
                }

                Console.ForegroundColor = ConsoleColor.White;
            }

            //Commit changes
            var conn = session.GetDbConnection();
            if (queueDinos.Count > 0)
                conn.arkentries_dinos.BulkWrite(queueDinos);
            if (queueItems.Count > 0)
                conn.arkentries_items.BulkWrite(queueItems);
        }

        private static void QueueEntryDb<T>(List<WriteModel<DbArkEntry<T>>> queue, T payload, string classname, string mod)
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
    }
}
