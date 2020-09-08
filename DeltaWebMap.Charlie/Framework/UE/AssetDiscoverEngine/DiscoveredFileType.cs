using System;
using System.Collections.Generic;
using System.Text;

namespace DeltaWebMap.Charlie.Framework.UE.AssetDiscoverEngine
{
    public enum DiscoveredFileType
    {
        None = -1, //File did not match any known types
        Unreadable = -2, //File was corrupted in some way and it failed to read
        Undeteremined = 0, //File could be one of the normal parts, but we aren't sure which
        Dino = 1,
        Item = 2,
        Structure = 3
    }
}
