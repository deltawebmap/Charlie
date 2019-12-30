using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeltaWebMap.Charlie.Framework.UE.Assets.UAssetTypes.TextureMods
{
    public abstract class BaseTextureMod
    {
        public abstract void Run(Image<Rgba32> img);
    }
}
