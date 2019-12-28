using System;
using System.Collections.Generic;
using System.Text;
using DeltaWebMap.Charlie.Framework.UE.Assets;
using DeltaWebMap.Charlie.Framework.UE.PropertyReader.Properties;

namespace DeltaWebMap.Charlie.Framework.UE.PropertyReader.Structs
{
    public class IntPointStruct : BaseStruct
    {
        public int p1;
        public int p2;

        public override string GetDebugString()
        {
            return $"p1={p1}, p2={p2}";
        }

        public override void ReadStruct(IOMemoryStream ms, UAssetFile f, StructProperty prop)
        {
            p1 = ms.ReadInt();
            p2 = ms.ReadInt();
        }
    }
}
