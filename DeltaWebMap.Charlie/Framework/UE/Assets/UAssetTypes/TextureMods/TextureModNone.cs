using System;
using System.Collections.Generic;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace DeltaWebMap.Charlie.Framework.UE.Assets.UAssetTypes.TextureMods
{
    public class TextureModNone : BaseTextureMod
    {
        public override void Run(Image<Rgba32> img)
        {
            //We literally do nothing...
        }
    }
}
