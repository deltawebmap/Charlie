using DeltaWebMap.Charlie.Framework.UE.Assets;
using DeltaWebMap.Charlie.Framework.UE.PropertyReader.Properties;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeltaWebMap.Charlie.Framework.UE.PropertyReader
{
    public abstract class BaseStruct
    {
        public abstract void ReadStruct(IOMemoryStream ms, UAssetFile f, StructProperty prop);
        public abstract string GetDebugString();
    }
}
