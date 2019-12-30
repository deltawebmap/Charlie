using DeltaWebMap.Charlie.Converters;
using DeltaWebMap.Charlie.Framework;
using DeltaWebMap.Charlie.Framework.Exceptions;
using DeltaWebMap.Charlie.Framework.Firebase;
using DeltaWebMap.Charlie.Framework.UE;
using DeltaWebMap.Charlie.Framework.UE.AssetDiscoverEngine;
using DeltaWebMap.Charlie.Framework.UE.Assets;
using DeltaWebMap.Charlie.Framework.UE.Assets.UAssetTypes;
using DeltaWebMap.Charlie.Framework.UE.PropertyReader;
using Newtonsoft.Json;
using System;
using System.IO;

namespace DeltaWebMap.Charlie
{
    class Program
    {
        const string DEBUG_INSTALL_ARK = @"E:\SteamLibrary\steamapps\common\ARK\ShooterGame\Content\";
        const string DEBUG_INSTALL_ADK = @"E:\Programs\ARKEditor\Projects\ShooterGame\Content\";

        const string DEBUG_FILE_1 = @"E:\SteamLibrary\steamapps\common\ARK\ShooterGame\Content\PrimalEarth\Dinos\Raptor\Raptor_Character_BP.uasset";
        const string DEBUG_FILE_6 = @"E:\SteamLibrary\steamapps\common\ARK\ShooterGame\Content\PrimalEarth\Dinos\Argentavis\Argent_Character_BP.uasset";
        const string DEBUG_FILE_7 = @"E:\SteamLibrary\steamapps\common\ARK\ShooterGame\Content\Mods\1905186031\DeltaSyncClient.uasset";
        const string DEBUG_FILE_3 = @"E:\SteamLibrary\steamapps\common\ARK\ShooterGame\Content\PrimalEarth\Dinos\Raptor\Sounds\s_raptor_call.uasset";
        const string DEBUG_FILE_5 = @"E:\SteamLibrary\steamapps\common\ARK\ShooterGame\Content\PrimalEarth\Dinos\Raptor\Raptor_layered.uasset";
        const string DEBUG_FILE_2 = @"E:\Programs\ARKEditor\Projects\ShooterGame\Content\Mods\DeltaServer\ActorQueues\BaseActorQueueBP.uasset";
        const string DEBUG_FILE_4 = @"E:\Programs\ARKEditor\Projects\ShooterGame\Content\Mods\DeltaServer\ActorQueues\StructureActorQueue.uasset";
        const string DEBUG_FILE_8 = @"E:\Programs\ARKEditor\Projects\ShooterGame\Content\Mods\Tests\ComponentTestActor.uasset";

        static void Main(string[] args)
        {
            UEInstall install = new UEInstall(DEBUG_INSTALL_ARK);
            CharlieConfig config = JsonConvert.DeserializeObject<CharlieConfig>(File.ReadAllText(@"C:\Users\Roman\Documents\delta_dev\other\Charlie\config.json"));
            CharliePersist persist = new CharliePersist(config);
            persist.Load();

            //UAssetBlueprint bpt = install.OpenBlueprint(@"E:\SteamLibrary\steamapps\common\ARK\ShooterGame\Content\PrimalEarth\CoreBlueprints\PlayerPawnTest.uasset");
            //bpt.DebugWrite();
            //bp.defaults.props.Sort(new Comparison<BaseProperty>((x, y) => x.name.CompareTo(y.name)));
            //bp.myDefaults.props.Sort(new Comparison<BaseProperty>((x, y) => x.name.CompareTo(y.name)));
            /*foreach (var d in bp.defaults.props)
            {
                Console.WriteLine(d.name + " - " + d.type + " - " + d.GetDebugString());
            }*/

            CharlieSession session = new CharlieSession(install, config);
            session.Run();

            //install.OpenTexture2D(@"E:\Programs\ARKEditor\Projects\ShooterGame\Content\PrimalEarth\UI\Empty_RaptorHead_Icon.uasset");
            //UAssetTexture2D tex = install.OpenTexture2D(@"E:\SteamLibrary\steamapps\common\ARK\ShooterGame\Content\Aberration\Icons\Dinos\Empty_RockDrakeHead_Icon.uasset");

            //FirebaseService fs = new FirebaseService(config.firebase_cfg);

            //session.assetManager.AddTexture2D(tex);
            session.EndSession();

            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
