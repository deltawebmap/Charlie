using System;
using System.Collections.Generic;
using System.Text;

namespace DeltaWebMap.Charlie.Framework.UE.Assets
{
    public class GameObjectTableHead
    {
        public long startPos;

        public string coreType; //Seems to be script or UObject so far
        public int unknown1;
        public string objectType; //Class sometimes, other when coreType is script
        public int unknown2;
        public int index; //Index
        public string name; //Name used by the game
        public int unknown4;

        public static GameObjectTableHead ReadEntry(IOMemoryStream ms, UAssetFile f)
        {
            //Read in
            GameObjectTableHead g = new GameObjectTableHead();
            g.startPos = ms.position;
            g.coreType = ms.ReadNameTableEntry(f);
            g.unknown1 = ms.ReadInt();
            g.objectType = ms.ReadNameTableEntry(f);
            g.unknown2 = ms.ReadInt();
            g.index = ms.ReadInt();
            g.name = ms.ReadNameTableEntry(f);
            g.unknown4 = ms.ReadInt();
            return g;
        }

        /// <summary>
        /// Follows the index path up to get the original item, which usually contains the pathname to the file referenced.
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public GameObjectTableHead GetUnderlyingHead(UAssetFile f)
        {
            GameObjectTableHead h = this;
            while(h.index < 0)
            {
                int next = (-h.index) - 1;
                h = f.gameObjectReferences[next];
            }
            return h;
        }

        /// <summary>
        /// Checks if the file this references is a file we can read (in other words, does it start with /Game/).
        /// </summary>
        /// <returns></returns>
        public bool IsValidFile()
        {
            return name.StartsWith("/Game/");
        }

        /// <summary>
        /// Gets the referenced file, if name starts with /Game/.
        /// </summary>
        /// <returns></returns>
        public UENamespaceFile GetReferencedFile(UAssetFile f)
        {
            return f.install.GetFileFromGamePath(name+".uasset");
        }
    }
}
