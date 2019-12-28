using System;
using System.Collections.Generic;
using System.Text;
using DeltaWebMap.Charlie.Framework.UE.Assets;
using DeltaWebMap.Charlie.Framework.UE.PropertyReader.Properties;

namespace DeltaWebMap.Charlie.Framework.UE.PropertyReader.Structs
{
    public class UniqueNetIdStruct : BaseStruct
    {
        public int unk;
        public string netId;

        public override string GetDebugString()
        {
            return $"unk={unk}, netId={netId}";
        }

        public override void ReadStruct(IOMemoryStream ms, UAssetFile f, StructProperty prop, ArrayProperty array)
        {
            unk = ms.ReadInt();
            netId = ms.ReadUEString();
        }
    }
}
