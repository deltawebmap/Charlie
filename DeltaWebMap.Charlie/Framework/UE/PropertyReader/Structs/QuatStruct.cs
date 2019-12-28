using System;
using System.Collections.Generic;
using System.Text;
using DeltaWebMap.Charlie.Framework.UE.Assets;
using DeltaWebMap.Charlie.Framework.UE.PropertyReader.Properties;

namespace DeltaWebMap.Charlie.Framework.UE.PropertyReader.Structs
{
    public class QuatStruct : BaseStruct
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public override string GetDebugString()
        {
            return $"x={x}, y={y}, z={z}, w={w}";
        }

        public override void ReadStruct(IOMemoryStream ms, UAssetFile f, StructProperty prop, ArrayProperty array)
        {
            x = ms.ReadFloat();
            y = ms.ReadFloat();
            z = ms.ReadFloat();
            w = ms.ReadFloat();
        }
    }
}
