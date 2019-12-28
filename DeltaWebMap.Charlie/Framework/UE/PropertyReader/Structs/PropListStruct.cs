using System;
using System.Collections.Generic;
using System.Text;
using DeltaWebMap.Charlie.Framework.UE.Assets;
using DeltaWebMap.Charlie.Framework.UE.PropertyReader.Properties;

namespace DeltaWebMap.Charlie.Framework.UE.PropertyReader.Structs
{
    public class PropListStruct : BaseStruct
    {
        public UPropertyGroup properties;

        public override string GetDebugString()
        {
            return $"properties(COUNT)={properties.props.Count}";
        }

        public override void ReadStruct(IOMemoryStream ms, UAssetFile f, StructProperty prop, ArrayProperty array)
        {
            properties = new UPropertyGroup();
            properties.ReadProps(ms, f, null);
        }
    }
}
