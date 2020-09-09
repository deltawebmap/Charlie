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
            namespaceRemaps = new Dictionary<string, string>();
        }

        public DirectoryInfo info;
        public Dictionary<string, UAssetBlueprint> blueprintCache;
        private Dictionary<string, string> namespaceRemaps; //Used for mod installations. Remaps from the key to the value

        public void AddRemap(string from, string to)
        {
            namespaceRemaps.Add(from, to);
        }

        /// <summary>
        /// Runs through the remappings and returns the result. This converts from a path ARK uses to a path that can be read on the filesystem
        /// </summary>
        /// <param name="pathname"></param>
        /// <returns></returns>
        public string RemapGamePath(string pathname)
        {
            //Search for remappings
            foreach(var r in namespaceRemaps)
            {
                if(pathname.StartsWith(r.Key))
                {
                    pathname = r.Value + pathname.Substring(r.Key.Length);
                }
            }
            return pathname;
        }

        /// <summary>
        /// Does the reverse of what RemapGamePath does
        /// </summary>
        /// <param name="pathname"></param>
        /// <returns></returns>
        public string ReverseRemapGamePath(string pathname)
        {
            //Search for remappings
            foreach (var r in namespaceRemaps)
            {
                if (pathname.StartsWith(r.Value))
                {
                    pathname = r.Key + pathname.Substring(r.Value.Length);
                }
            }
            return pathname;
        }

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

            //Remap
            pathname = RemapGamePath(pathname);

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

            //Remap
            pathname = RemapGamePath(pathname);

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

        /// <summary>
        /// Opens a Material
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public UAssetMaterial OpenMaterial(string path)
        {
            //Read
            UAssetMaterial bp = new UAssetMaterial();
            bp.BaseReadFile(this, path);

            //Return it
            return bp;
        }

        /// <summary>
        /// Opens a Texture2D
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public UAssetTexture2D OpenTexture2D(string path)
        {
            //Read
            UAssetTexture2D bp = new UAssetTexture2D();
            bp.BaseReadFile(this, path);

            //Return it
            return bp;
        }
    }
}
