using System;
using System.Collections.Generic;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace DeltaWebMap.Charlie.Framework.UE.Assets.UAssetTypes.Textures
{
    public class Dxt5TextureDecoder : BaseTextureDecoder
    {
        public override Image<Rgba32> ComputeImage(IOMemoryStream ms, int width, int height)
        {
            //Read
            Image<Rgba32> img = new Image<Rgba32>(width, height);
            return null;
        }

    }
}
