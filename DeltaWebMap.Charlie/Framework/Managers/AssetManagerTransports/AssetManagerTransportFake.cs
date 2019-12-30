using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DeltaWebMap.Charlie.Framework.Managers.AssetManagerTransports
{
    public class AssetManagerTransportFake : AssetManagerTransport
    {
        CharlieConfig config;

        public override string AddFile(string pathname, Stream data)
        {
            Console.WriteLine("[FAKE ASSET MANAGER] Added file "+pathname+" with "+data.Length+" bytes.");
            /*using (FileStream fs = new FileStream(config.temp + pathname, FileMode.Create))
                data.CopyTo(fs);*/
            return pathname;
        }

        public override void EndSession()
        {
            Console.WriteLine("[FAKE ASSET MANAGER] Ended session.");
        }

        public override void StartSession(CharlieConfig cfg)
        {
            Console.WriteLine("[FAKE ASSET MANAGER] Created session.");
            config = cfg;
        }
    }
}
