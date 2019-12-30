using System;
using System.Collections.Generic;
using System.Text;
using DeltaWebMap.Charlie.Framework.UE.Assets;
using DeltaWebMap.Charlie.Framework.UE.Assets.UAssetTypes;

namespace DeltaWebMap.Charlie.Framework.UE.PropertyReader.Properties
{
    public class ObjectProperty : BaseProperty
    {
        public int objectIndex;
        private UAssetFile context;

        public override string GetDebugString()
        {
            return $"objectIndex={objectIndex}";
        }

        public override void Link(UPropertyGroup g, UAssetFile file)
        {
            context = file;
        }

        public override void Read(IOMemoryStream ms, UAssetFile file)
        {
            if (length != 4 && !isArray)
                throw new Exception("DEBUG: ObjectProperty length != 4, it was " + length + " instead!");
            objectIndex = ms.ReadInt();
        }

        public GameObjectTableHead GetReferencedHead()
        {
            if (objectIndex >= 0)
                throw new Exception("This is not a referenced head. Try using GetEmbeddedReferencedHead instead.");
            int index = (-objectIndex) - 1;
            return context.gameObjectReferences[index];
        }

        public EmbeddedGameObjectTableHead GetEmbeddedReferencedHead()
        {
            if (objectIndex < 0)
                throw new Exception("This is not an embedded referenced head. Try using GetReferencedHead instead.");
            int index = (objectIndex) - 1;
            return context.gameObjectEmbeds[index];
        }

        public UAssetBlueprint GetReferencedBlueprintAsset()
        {
            var h = GetReferencedHead().GetUnderlyingHead(context).GetReferencedFile(context);
            return context.install.OpenBlueprint(h.GetFilename());
        }

        public UAssetMaterial GetReferencedMaterialAsset()
        {
            var h = GetReferencedHead().GetUnderlyingHead(context).GetReferencedFile(context);
            return context.install.OpenMaterial(h.GetFilename());
        }

        public UAssetTexture2D GetReferencedTexture2DAsset()
        {
            var h = GetReferencedHead().GetUnderlyingHead(context).GetReferencedFile(context);
            return context.install.OpenTexture2D(h.GetFilename());
        }

        public bool GetIsValid()
        {
            return objectIndex != 0;
        }
    }
}
