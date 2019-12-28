using System;
using System.Collections.Generic;
using System.Text;
using DeltaWebMap.Charlie.Framework.UE.Assets;

namespace DeltaWebMap.Charlie.Framework.UE.PropertyReader.Properties
{
    public class ArrayProperty : BaseProperty
    {
        public string arrayFieldType;
        public int arrayFieldTypeIndex;
        public int arrayItemCount;
        public BaseProperty[] properties;

        public override string GetDebugString()
        {
            return $"arrayFieldType={arrayFieldType}, arrayFieldTypeIndex={arrayFieldTypeIndex}, arrayItemCount={arrayItemCount}";
        }

        public override void Link(UPropertyGroup g, UAssetFile file)
        {
            //No linking needed...
        }

        public override void Read(IOMemoryStream ms, UAssetFile file)
        {
            //Check the array size
            if (length < 4)
                throw new Exception("Array size is required.");

            //Read field type
            arrayFieldType = ms.ReadNameTableEntry(file);
            arrayFieldTypeIndex = ms.ReadInt();

            //Read number of elements
            arrayItemCount = ms.ReadInt();

            //Read all properties
            properties = new BaseProperty[arrayItemCount];
            for (int i = 0; i < arrayItemCount; i++)
                properties[i] = BaseProperty.ReadProperty(ms, file, this);
        }
    }
}
