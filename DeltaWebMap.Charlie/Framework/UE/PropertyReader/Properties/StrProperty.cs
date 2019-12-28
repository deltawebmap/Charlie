using System;
using System.Collections.Generic;
using System.Text;
using DeltaWebMap.Charlie.Framework.UE.Assets;

namespace DeltaWebMap.Charlie.Framework.UE.PropertyReader.Properties
{
    public class StrProperty : BaseProperty
    {
        public string data;

        public override string GetDebugString()
        {
            return $"data=\"{data}\"";
        }

        public override void Link(UPropertyGroup g, UAssetFile file)
        {
            //No linking needed...
        }

        public override void Read(IOMemoryStream ms, UAssetFile file)
        {
            data = ms.ReadUEString();
        }
    }
}
