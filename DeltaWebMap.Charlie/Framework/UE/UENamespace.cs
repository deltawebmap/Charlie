using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DeltaWebMap.Charlie.Framework.UE
{
    public class UENamespace
    {
        public UEInstall install;
        public DirectoryInfo info;

        public UENamespace(UEInstall install, string path)
        {
            this.install = install;
            this.info = new DirectoryInfo(path);
        }
    }
}
