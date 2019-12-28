using System;
using System.Collections.Generic;
using System.Text;
using DeltaWebMap.Charlie.Framework.UE.Assets;

namespace DeltaWebMap.Charlie.Framework.UE.PropertyReader.Properties
{
    public class BoolProperty : BaseProperty
    {
        public bool value;

        public override string GetDebugString()
        {
            return $"value={value}";
        }

        public override void Link(UPropertyGroup g, UAssetFile file)
        {
            //No linking needed.
        }

        public override void Read(IOMemoryStream ms, UAssetFile file)
        {
            value = ms.ReadByteBool();
        }
    }
}
