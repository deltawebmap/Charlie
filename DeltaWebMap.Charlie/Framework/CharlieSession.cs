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
        public const string NOTIFY_USER_ID = "5de41168d71dce61d4db5071";

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

        public void EndSession()
        {
            assetManager.FinalizeItems();

            //Send notification and log
            var conn = GetDbConnection();
            conn.GetRPC().SendRPCMessageToUser(LibDeltaSystem.RPC.RPCOpcode.PutNotification, DeltaRPCConnection.GetNotificationPayload(new LibDeltaSystem.Entities.Notifications.PushNotificationDisplayInfo
            {
                title = "Charlie Commit Pushed",
                type = LibDeltaSystem.Entities.Notifications.PushNotificationType.Generic,
                status_icon = LibDeltaSystem.Entities.Notifications.PushNotificationStatusIcon.Alert,
                text = $"Commit {id.ToString()} deployed to {conn.config.env} in {Math.Round((DateTime.UtcNow - beginTime).TotalSeconds)}s; {entriesScanned} scanned, {entriesUpdated} updated, {assetsUploaded} assets",
                big_icon_url = null
            }, null), NOTIFY_USER_ID, RPCType.Notification);
        }

        public DeltaConnection GetDbConnection()
        {
            DeltaConnection conn = new DeltaConnection(config.delta_cfg, "CHARLIE-DEPLOY", 1, 1);
            conn.Connect().GetAwaiter().GetResult();
            return conn;
        }

        public void Log(string topic, string msg)
        {
            //Do a very simple hash of this to determine the color
            int total = 0;
            foreach (char c in topic)
                total += (byte)c;
            total = (total % 15) + 1;
            ConsoleColor color = (ConsoleColor)total;

            //Write
            Console.ForegroundColor = color;
            Console.WriteLine($"[{topic}] {msg}");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
