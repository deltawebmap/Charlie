using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DeltaWebMap.Charlie.Framework.UE
{
    public class UENamespaceFile
    {
        public UEInstall install;
        public UENamespace parentNamespace;
        public FileInfo info;

        public string GetFilename()
        {
            return info.FullName;
        }

        public UENamespaceFile(UEInstall install, string path)
        {
            this.install = install;
            this.info = new FileInfo(path);
            this.parentNamespace = new UENamespace(install, info.Directory.FullName);

            if (!this.info.Exists)
                throw new Exception("Failed to find referenced file " + path + "!");
        }
    }
}
