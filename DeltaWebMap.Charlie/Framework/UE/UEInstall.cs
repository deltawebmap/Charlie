using DeltaWebMap.Charlie.Framework.UE.Assets.UAssetTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DeltaWebMap.Charlie.Framework.UE
{
    /// <summary>
    /// Represents an installation of a game, starting from the /Content/ dir
    /// </summary>
    public class UEInstall
    {
        public UEInstall(string contentPath)
        {
            info = new DirectoryInfo(contentPath);
            blueprintCache = new Dictionary<string, UAssetBlueprint>();
        }

        public DirectoryInfo info;
        public Dictionary<string, UAssetBlueprint> blueprintCache;

        /// <summary>
        /// Gets a file from a game path. Game paths will always begin in "/Game/".
        /// </summary>
        /// <param name="pathname"></param>
        /// <returns></returns>
        public UENamespaceFile GetFileFromGamePath(string pathname)
        {
            //Verify
            if (!pathname.StartsWith("/Game/"))
                throw new Exception("This is not a namespace path.");

            //Get path
            string path = info.FullName + pathname.Substring("/Game/".Length);

            //Get it
            return new UENamespaceFile(this, path);
        }

        /// <summary>
        /// Gets a namespace from a game path. Game paths will always begin in "/Game/".
        /// </summary>
        /// <param name="pathname"></param>
        /// <returns></returns>
        public UENamespace GetNamespaceFromGamePath(string pathname)
        {
            //Verify
            if (!pathname.StartsWith("/Game/"))
                throw new Exception("This is not a namespace path.");

            //Get path
            string path = info.FullName + pathname.Substring("/Game/".Length);

            //Get it
            return new UENamespace(this, path);
        }

        /// <summary>
        /// Opens a Blueprint
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public UAssetBlueprint OpenBlueprint(string path)
        {
            //Check if it exists in the cache
            if (blueprintCache.ContainsKey(path))
                return blueprintCache[path];

            //Read
            UAssetBlueprint bp = new UAssetBlueprint();
            bp.BaseReadFile(this, path);

            //Insert into the cache
            blueprintCache.Add(path, bp);

            //Return it
            return bp;
        }
    }
}
