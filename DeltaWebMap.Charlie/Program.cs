using DeltaWebMap.Charlie.Converters;
using DeltaWebMap.Charlie.Framework;
using DeltaWebMap.Charlie.Framework.Exceptions;
using DeltaWebMap.Charlie.Framework.Firebase;
using DeltaWebMap.Charlie.Framework.UE;
using DeltaWebMap.Charlie.Framework.UE.AssetDiscoverEngine;
using DeltaWebMap.Charlie.Framework.UE.Assets;
using DeltaWebMap.Charlie.Framework.UE.Assets.UAssetTypes;
using DeltaWebMap.Charlie.Framework.UE.PropertyReader;
using LibDeltaSystem;
using Newtonsoft.Json;
using System;
using System.IO;

namespace DeltaWebMap.Charlie
{
    class Program
    {
        const string DEBUG_INSTALL_ARK = @"C:\Program Files (x86)\Steam\steamapps\common\ARK\ShooterGame\Content\";
        const string DEBUG_INSTALL_ADK = @"E:\Programs\ARKEditor\Projects\ShooterGame\Content\";

        const string DEBUG_FILE_1 = @"E:\SteamLibrary\steamapps\common\ARK\ShooterGame\Content\PrimalEarth\Dinos\Raptor\Raptor_Character_BP.uasset";
        const string DEBUG_FILE_6 = @"E:\SteamLibrary\steamapps\common\ARK\ShooterGame\Content\PrimalEarth\Dinos\Argentavis\Argent_Character_BP.uasset";
        const string DEBUG_FILE_7 = @"E:\SteamLibrary\steamapps\common\ARK\ShooterGame\Content\Mods\1905186031\DeltaSyncClient.uasset";
        const string DEBUG_FILE_3 = @"E:\SteamLibrary\steamapps\common\ARK\ShooterGame\Content\PrimalEarth\Dinos\Raptor\Sounds\s_raptor_call.uasset";
        const string DEBUG_FILE_5 = @"E:\SteamLibrary\steamapps\common\ARK\ShooterGame\Content\PrimalEarth\Dinos\Raptor\Raptor_layered.uasset";
        const string DEBUG_FILE_2 = @"E:\Programs\ARKEditor\Projects\ShooterGame\Content\Mods\DeltaServer\ActorQueues\BaseActorQueueBP.uasset";
        const string DEBUG_FILE_4 = @"E:\Programs\ARKEditor\Projects\ShooterGame\Content\Mods\DeltaServer\ActorQueues\StructureActorQueue.uasset";
        const string DEBUG_FILE_8 = @"E:\Programs\ARKEditor\Projects\ShooterGame\Content\Mods\Tests\ComponentTestActor.uasset";

        public static CharlieConfig config;
        public static DeltaDatabaseConnection db;

        static void Main(string[] args)
        {
            //Load config
            config = JsonConvert.DeserializeObject<CharlieConfig>(File.ReadAllText(@"C:\Users\Roman\Documents\DeltaWebMap\Charlie\config.json"));

            //Create Delta connection
            db = DeltaDatabaseConnection.OpenFromDeltaConfig(config.delta_cfg);

            //Load persistent data
            CharliePersist persist = new CharliePersist(config);
            persist.Load();

            //Create install
            UEInstall install = new UEInstall(DEBUG_INSTALL_ARK);
            
            //Begin and run session
            CharlieSession session = new CharlieSession(install, config);
            session.Run();

            //Done
            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
