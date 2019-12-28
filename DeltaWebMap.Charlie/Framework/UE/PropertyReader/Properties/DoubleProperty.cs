using System;
using System.Collections.Generic;
using System.Text;
using DeltaWebMap.Charlie.Framework.UE.Assets;

namespace DeltaWebMap.Charlie.Framework.UE.PropertyReader.Properties
{
    public class DoubleProperty : BaseProperty
    {
        public double value;

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
            value = ms.ReadDouble();
        }
    }
}
