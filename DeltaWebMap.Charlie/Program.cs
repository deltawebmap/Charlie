using DeltaWebMap.Charlie.Framework;
using DeltaWebMap.Charlie.Framework.Exceptions;
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
            CharlieConfig config = JsonConvert.DeserializeObject<CharlieConfig>(File.ReadAllText("example_config.json"));
            CharlieSession session = new CharlieSession(config);
            session.Load();

            //UAssetBlueprint bpt = install.OpenBlueprint(@"E:\SteamLibrary\steamapps\common\ARK\ShooterGame\Content\PrimalEarth\CoreBlueprints\PlayerPawnTest.uasset");
            //bpt.DebugWrite();
            //bp.defaults.props.Sort(new Comparison<BaseProperty>((x, y) => x.name.CompareTo(y.name)));
            //bp.myDefaults.props.Sort(new Comparison<BaseProperty>((x, y) => x.name.CompareTo(y.name)));
            /*foreach (var d in bp.defaults.props)
            {
                Console.WriteLine(d.name + " - " + d.type + " - " + d.GetDebugString());
            }*/

            //Seek the files
            AssetSeeker s = new AssetSeeker(install, config.exclude_regex);
            var files = s.SeekAssets(session);

            //Now run each file
            foreach(var f in files)
            {
                Console.WriteLine(f.Key);
                try
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    UAssetBlueprint bp = install.OpenBlueprint(f.Key);
                    Console.ForegroundColor = ConsoleColor.White;
                } catch (FailedToFindDefaultsException)
                {
                    Console.WriteLine("FAILED TO FIND DEFAULTS");
                }
            }

            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
