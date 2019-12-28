using System;
using System.Collections.Generic;
using System.Text;
using DeltaWebMap.Charlie.Framework.UE.Assets;

namespace DeltaWebMap.Charlie.Framework.UE.PropertyReader.Properties
{
    public class NameProperty : BaseProperty
    {
        public string valueName;
        public int valueIndex;

        public override string GetDebugString()
        {
            return $"valueName={valueName}, valueIndex={valueIndex}";
        }

        public override void Link(UPropertyGroup g, UAssetFile file)
        {
            //No linking needed...
        }

        public override void Read(IOMemoryStream ms, UAssetFile file)
        {
            if (isArray || length == 8)
            {
                valueName = ms.ReadNameTableEntry(file);
                valueIndex = ms.ReadInt();
            }
            else
            {
                valueName = ms.ReadNameTableEntry(file);
            }
        }
    }
}
