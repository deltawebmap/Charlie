using DeltaWebMap.Charlie.Framework.UE.PropertyReader;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DeltaWebMap.Charlie.Framework.UE.Assets.UAssetTypes
{
    public class UAssetTexture2D : UAssetFile
    {
        /// <summary>
        /// Holds texture properties
        /// </summary>
        public UPropertyGroup properties;

        private const string UMODEL_EXE_PATH = "Framework/Lib/UModel/umodel.exe";

        public override void BaseReadFile(UEInstall install, string path)
        {
            base.BaseReadFile(install, path);
        }

        void ReadProperties()
        {
            //The properties have the same name as we do
            EmbeddedGameObjectTableHead h = GetEmbedByTypeName(classname);
            UPropertyGroup group = ReadUPropertyGroupFromObject(h);
            properties = group;
        }

        /// <summary>
        /// Gets the image data
        /// </summary>
        /// <returns></returns>
        public Image<Rgba32> GetImage(CharlieConfig cfg)
        {
            //For now, this is simply a wrapper for UModel

            //Get the executable
            string umodel = UMODEL_EXE_PATH;
            if (!File.Exists(umodel))
                throw new Exception("Failed to find umodel.exe. Make sure you've downloaded the umodel binary and placed it in " + UMODEL_EXE_PATH);

            //Get our export folder
            Guid idExport = Guid.NewGuid();
            string export = cfg.temp + idExport.ToString() + "\\";

            //Copy here (yuck)
            Directory.CreateDirectory(export);
            File.Copy(file.GetFilename(), export + classname + ".uasset");

            //Prepare our launch command
            string launch = $"-game=ark -export -notgacomp \"{export + classname + ".uasset"}\"";

            //Launch the process
            Process proc = Process.Start(new ProcessStartInfo
            {
                Arguments = launch,
                FileName = umodel,
                WorkingDirectory = export,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true
            });
            proc.WaitForExit();

            //Open and convert
            Image<Rgba32> img;
            using (FileStream fs = new FileStream(export+ "UmodelExport\\"+classname+".tga", FileMode.Open, FileAccess.Read))
            {
                //Now, begin reading the TGA data https://en.wikipedia.org/wiki/Truevision_TGA
                IOMemoryStream imgReader = new IOMemoryStream(fs, true);
                imgReader.position += 3 + 5; //Skip intro, it will always be known
                imgReader.ReadShort(); //Will always be 0
                imgReader.ReadShort(); //Will aways be 0
                short width = imgReader.ReadShort();
                short height = imgReader.ReadShort();
                byte colorDepth = imgReader.ReadByte();
                imgReader.ReadByte();

                //Create image
                img = new Image<Rgba32>(width, height);

                //Now, we can begin reading image data
                //This appears to be bugged for non-square images right now. (VERIFY)
                //Read file
                byte[] channels;
                for (int y = 0; y < width; y++)
                {
                    for (int x = 0; x < height; x++)
                    {
                        if (colorDepth == 32)
                        {
                            //Read four channels
                            channels = imgReader.ReadBytes(4);

                            //Set pixel
                            img[x, width - y - 1] = new Rgba32(channels[2], channels[1], channels[0], channels[3]);
                        }
                        else if (colorDepth == 24)
                        {
                            //Read three channels
                            channels = imgReader.ReadBytes(3);

                            //Set pixel
                            img[x, width - y - 1] = new Rgba32(channels[2], channels[1], channels[0]);
                        }
                    }
                }
            }

            //Clean up
            Directory.Delete(export, true);

            return img;
        }

        /// <summary>
        /// Calculates the SHA256 and returns it.
        /// </summary>
        /// <returns></returns>
        public byte[] GetSHA256()
        {
            byte[] checksum;
            using (FileStream fs = new FileStream(pathname, FileMode.Open, FileAccess.Read))
            {
                var sha = new SHA256Managed();
                checksum = sha.ComputeHash(fs);
            }
            return checksum;
        }
    }
}
