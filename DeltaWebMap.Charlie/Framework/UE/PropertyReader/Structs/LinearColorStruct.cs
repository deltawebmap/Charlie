using System;
using System.Collections.Generic;
using System.Text;
using DeltaWebMap.Charlie.Framework.UE.Assets;
using DeltaWebMap.Charlie.Framework.UE.PropertyReader.Properties;

namespace DeltaWebMap.Charlie.Framework.UE.PropertyReader.Structs
{
    public class LinearColorStruct : BaseStruct
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public override string GetDebugString()
        {
            return $"r={r}, g={g}, b={b}, a={a}";
        }

        public override void ReadStruct(IOMemoryStream ms, UAssetFile f, StructProperty prop)
        {
            r = ms.ReadFloat();
            g = ms.ReadFloat();
            b = ms.ReadFloat();
            a = ms.ReadFloat();
        }
    }
}
