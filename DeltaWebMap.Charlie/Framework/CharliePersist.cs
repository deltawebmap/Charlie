using DeltaWebMap.Charlie.Framework.Persist;
using DeltaWebMap.Charlie.Framework.Persist.Db;
using LibDeltaSystem.Entities.ArkEntries;
using LiteDB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DeltaWebMap.Charlie.Framework
{
    public class CharliePersist
    {
        public CharlieConfig config;

        public LiteDatabase db;
        public LiteCollection<CDbUploadedAsset> db_uploaded_assets;
        public LiteCollection<CDbTexture2D> db_uploaded_texture;
        public DiscoveryFile discovery;

        public CharliePersist(CharlieConfig config)
        {
            this.config = config;
            this.db = new LiteDatabase(config.persist + "charlie.db");
            this.db_uploaded_assets = db.GetCollection<CDbUploadedAsset>("uploaded_assets");
            this.db_uploaded_texture = db.GetCollection<CDbTexture2D>("uploaded_textures");
        }

        public void Load()
        {
            discovery = FileLoaderHelper<DiscoveryFile>(config.persist + "discovered_files.json");
        }

        public void Save()
        {
            FileSaveHelper(config.persist + "discovered_files.json", discovery);
        }

        private T FileLoaderHelper<T>(string path)
        {
            if (!File.Exists(path))
                return (T)Activator.CreateInstance(typeof(T));
            else
                return JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
        }

        private void FileSaveHelper<T>(string path, T data)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(data, Formatting.Indented));
        }

        public void InfoLog(string name, string value)
        {
            Console.WriteLine($"[{name}] {value}");
        }
    }
}
