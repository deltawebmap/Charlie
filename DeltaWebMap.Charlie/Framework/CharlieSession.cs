using DeltaWebMap.Charlie.Framework.Persist;
using LiteDB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DeltaWebMap.Charlie.Framework
{
    public class CharlieSession
    {
        public CharlieConfig config;

        public LiteDatabase db;
        public DiscoveryFile discovery;

        public CharlieSession(CharlieConfig config)
        {
            this.config = config;
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
