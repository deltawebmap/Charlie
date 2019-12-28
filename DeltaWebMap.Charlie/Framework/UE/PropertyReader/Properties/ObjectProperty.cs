using System;
using System.Collections.Generic;
using System.Text;
using DeltaWebMap.Charlie.Framework.UE.Assets;

namespace DeltaWebMap.Charlie.Framework.UE.PropertyReader.Properties
{
    public class ObjectProperty : BaseProperty
    {
        public int objectIndex;

        public override string GetDebugString()
        {
            return $"objectIndex={objectIndex}";
        }

        public override void Link(UPropertyGroup g, UAssetFile file)
        {
            //Linking SHOULD be done, but not yet!
        }

        public override void Read(IOMemoryStream ms, UAssetFile file)
        {
            if (length != 4 && !isArray)
                throw new Exception("DEBUG: ObjectProperty length != 4, it was " + length + " instead!");
            objectIndex = ms.ReadInt();
        }

        public GameObjectTableHead GetReferencedHead(UAssetFile file)
        {
            if (objectIndex >= 0)
                throw new Exception("This is not a referenced head. Try using GetEmbeddedReferencedHead instead.");
            int index = (-objectIndex) - 1;
            return file.gameObjectReferences[index];
        }

        public EmbeddedGameObjectTableHead GetEmbeddedReferencedHead(UAssetFile file)
        {
            if (objectIndex < 0)
                throw new Exception("This is not an embedded referenced head. Try using GetReferencedHead instead.");
            int index = (objectIndex) - 1;
            return file.gameObjectEmbeds[index];
        }
    }
}
