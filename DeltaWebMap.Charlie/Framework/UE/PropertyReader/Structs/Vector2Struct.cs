using System;
using System.Collections.Generic;
using System.Text;
using DeltaWebMap.Charlie.Framework.UE.Assets;
using DeltaWebMap.Charlie.Framework.UE.PropertyReader.Properties;

namespace DeltaWebMap.Charlie.Framework.UE.PropertyReader.Structs
{
    public class Vector2Struct : BaseStruct
    {
        public float x;
        public float y;

        public override string GetDebugString()
        {
            return $"x={x}, y={y}";
        }

        public override void ReadStruct(IOMemoryStream ms, UAssetFile f, StructProperty prop, ArrayProperty array)
        {
            x = ms.ReadFloat();
            y = ms.ReadFloat();
        }
    }
}
