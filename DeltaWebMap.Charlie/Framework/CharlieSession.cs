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
using System.IO;
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
            //Seek the files
            AssetSeeker s = new AssetSeeker(install, config.exclude_regex);
            var files = s.SeekAssets(persist);

            //Open main base game package
            ArkPackage mainPackage = new ArkPackage(this, 0, "BASE_GAME", "/Game/PrimalEarth/CoreBlueprints/PrimalGameData_BP.uasset", "DinoEntries");

            //Discover and load mod info
            ArkModPackage[] mods = LoadModsInfo();

            //Compile into a package list
            ArkPackage[] packages = new ArkPackage[mods.Length + 1];
            packages[0] = mainPackage;
            Array.Copy(mods, 0, packages, 1, mods.Length);

            //Load all PrimalGameDatas
            foreach (var p in packages)
            {
                Log("LoadPackages", $"Loading PrimalGameData for package '{p.title}' ({p.modId})...", ConsoleColor.Cyan);
                try
                {
                    p.InitPackage();
                    Log("LoadPackages", $"Loaded {p.dinoEntries.Count} dino entries from package successfully.", ConsoleColor.Green);
                } catch (Exception ex)
                {
                    Log("LoadPackages", $"Failed to load PrimalGameData for package! {ex.Message}", ConsoleColor.Red);
                }
            }

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

                //Find the package this belongs to
                ArkPackage filePackage = mainPackage; //We assume it belongs to the root game initially
                string gamePath = bp.file.GetGamePath();
                foreach(var p in packages)
                {
                    if (p.IsFileBelongingToPackage(gamePath))
                        filePackage = p;
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
                        DinosaurEntry entry = DinoConverter.ConvertDino(this, bp, filePackage.dinoEntries);
                        if (entry == null)
                            continue;
                        QueueEntryDb(filePackage.queueDinos, entry, entry.classname, filePackage.packageDinos);
                        classname = entry.classname;
                    }
                    else if (trueType == DiscoveredFileType.Item)
                    {
                        ItemEntry entry = ItemConverter.ConvertItem(this, bp);
                        if (entry == null)
                            continue;
                        QueueEntryDb(filePackage.queueItems, entry, entry.classname, filePackage.packageItems);
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

            //Commit changes to all packages
            foreach (var p in packages)
            {
                Log("CommitChanges", $"Commiting changes for package '{p.title}' ({p.modId})...", ConsoleColor.Cyan);
                p.CommitChanges();
                Log("CommitChanges", $"Committed {p.queueDinos.Count} dinos, {p.queueItems.Count} items successfully!", ConsoleColor.Green);
            }

            //Upload content
            Log("FinalizeChanges", "Uploading media...", ConsoleColor.Cyan);
            assetManager.FinalizeItems();
        }

        private ArkModPackage[] LoadModsInfo()
        {
            //Find all .mod files in the mods folder
            string[] modFiles = Directory.GetFiles(install.GetNamespaceFromGamePath("/Game/Mods/").info.FullName);

            //Loop through mods and load
            List<ArkModPackage> packages = new List<ArkModPackage>();
            foreach(var m in modFiles)
            {
                //Validate that this is a .mod file
                if (!m.EndsWith(".mod"))
                    continue;

                //Load the package
                ArkModPackage pack = ArkModPackage.GetArkModPackageFromMod(this, m);

                //Add
                if(pack.customProperties.ContainsKey("PrimalGameData"))
                    packages.Add(pack);
            }

            return packages.ToArray();
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

        public void Log(string topic, string msg, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"[{topic}] {msg}");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
