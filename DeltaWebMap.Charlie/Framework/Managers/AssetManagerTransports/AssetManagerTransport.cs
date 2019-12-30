using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DeltaWebMap.Charlie.Framework.Managers.AssetManagerTransports
{
    public abstract class AssetManagerTransport
    {
        public abstract void StartSession(CharlieConfig cfg);
        public abstract void EndSession();
        public abstract string AddFile(string pathname, Stream data);
    }
}
