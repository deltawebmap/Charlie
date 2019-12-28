using System;
using System.Collections.Generic;
using System.Text;
using DeltaWebMap.Charlie.Framework.UE.Assets;

namespace DeltaWebMap.Charlie.Framework.UE.PropertyReader.Properties
{
    public class TextProperty : BaseProperty
    {
        public string value;

        public override string GetDebugString()
        {
            return $"value={value}";
        }

        public override void Link(UPropertyGroup g, UAssetFile file)
        {
            //No linking needed...
        }

        public override void Read(IOMemoryStream ms, UAssetFile file)
        {
            //Not really sure how this works....this is just what my original program written a year ago ago does
            value = Convert.ToBase64String(ms.ReadBytes(length));
        }
    }
}
