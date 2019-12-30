using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeltaWebMap.Charlie.Framework.UE.Assets.UAssetTypes.Textures
{
    public abstract class BaseTextureDecoder
    {
        public abstract Image<Rgba32> ComputeImage(IOMemoryStream ms, int width, int height);
    }
}
