using System;
using System.Collections.Generic;
using System.Text;
using DeltaWebMap.Charlie.Framework.UE.Assets;
using DeltaWebMap.Charlie.Framework.UE.PropertyReader.Properties;

namespace DeltaWebMap.Charlie.Framework.UE.PropertyReader.Structs
{
    public class ColorStruct : BaseStruct
    {
        public byte b;
        public byte g;
        public byte r;
        public byte a;

        public override string GetDebugString()
        {
            return $"b={b}, g={g}, r={r}, a={a}";
        }

        public override void ReadStruct(IOMemoryStream ms, UAssetFile f, StructProperty prop)
        {
            b = ms.ReadByte();
            g = ms.ReadByte();
            r = ms.ReadByte();
            a = ms.ReadByte();
        }
    }
}
