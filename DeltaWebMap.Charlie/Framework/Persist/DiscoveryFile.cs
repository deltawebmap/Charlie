using DeltaWebMap.Charlie.Framework.UE.AssetDiscoverEngine;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeltaWebMap.Charlie.Framework.Persist
{
    public class DiscoveryFile
    {
        public DateTime time;
        public Dictionary<string, DiscoveredFileType> files;

        public DiscoveryFile()
        {
            time = DateTime.UtcNow;
            files = new Dictionary<string, DiscoveredFileType>();
        }
    }
}
