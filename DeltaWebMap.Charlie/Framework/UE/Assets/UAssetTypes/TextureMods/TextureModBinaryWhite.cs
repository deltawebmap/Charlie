using System;
using System.Collections.Generic;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace DeltaWebMap.Charlie.Framework.UE.Assets.UAssetTypes.TextureMods
{
    public class TextureModBinaryWhite : BaseTextureMod
    {
        public override void Run(Image<Rgba32> img)
        {
            //This mod will essentially just use the alpha channel, converting all other channels to white
            for(int x = 0; x<img.Width; x++)
            {
                for (int y = 0; y < img.Height; y++)
                {
                    img[x, y] = new Rgba32(255, 255, 255, img[x, y].A);
                }
            }
        }
    }
}
