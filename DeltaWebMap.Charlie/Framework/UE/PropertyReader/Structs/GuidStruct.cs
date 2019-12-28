using System;
using System.Collections.Generic;
using System.Text;
using DeltaWebMap.Charlie.Framework.UE.Assets;
using DeltaWebMap.Charlie.Framework.UE.PropertyReader.Properties;

namespace DeltaWebMap.Charlie.Framework.UE.PropertyReader.Structs
{
    public class GuidStruct : BaseStruct
    {
        public Guid uuid;

        public override string GetDebugString()
        {
            return $"uuid={uuid.ToString()}";
        }

        public override void ReadStruct(IOMemoryStream ms, UAssetFile f, StructProperty prop)
        {
            byte[] data = ms.ReadBytes(4 * 4);
            uuid = new Guid(data);
        }
    }
}
