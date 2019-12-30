using DeltaWebMap.Charlie.Framework.Managers.AssetManagerTransports;
using DeltaWebMap.Charlie.Framework.Persist.Db;
using DeltaWebMap.Charlie.Framework.UE.Assets.UAssetTypes;
using LibDeltaSystem.Entities.ArkEntries;
using LibDeltaSystem.Tools;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DeltaWebMap.Charlie.Framework.Managers
{
    public class AssetManager
    {
        public CharlieConfig config;
        public CharliePersist persist;
        public AssetManagerTransport transport;

        public AssetManager(CharlieConfig config, CharliePersist persist, AssetManagerTransport transport)
        {
            this.config = config;
            this.persist = persist;
            this.transport = transport;
        }

        /// <summary>
        /// Adds a Texture2d and returns it's URL package
        /// </summary>
        /// <param name="texture"></param>
        public DeltaAsset AddTexture2D(UAssetTexture2D texture)
        {
            //Get the SHA-256 hash code of this texture
            byte[] hash = texture.GetSHA256();
            
            //Get existing data for this texture (if any)
            CDbTexture2D metadata = persist.db_uploaded_texture.FindById(texture.file.GetGamePath());
            if(metadata != null)
            {
                //Compare the hash code to see if they match. If they do, return the existing data
                if (BinaryTool.CompareBytes(hash, metadata.sha256))
                    return metadata.asset;
            }

            //Get the image data
            Image<Rgba32> img = texture.GetImage(config);

            //Upload the file for the base
            string baseUrl;
            MemoryStream baseStream = new MemoryStream();

            //Write and rewind
            img.SaveAsPng(baseStream);
            baseStream.Position = 0;

            //Save
            baseUrl = AddFile(baseStream, "png");

            //Upload the compressed thumbnail for the base
            string thumbUrl;
            img.Mutate(x => x.Resize(64, 64));
            MemoryStream thumbStream = new MemoryStream();

            //Write and rewind
            img.SaveAsPng(thumbStream);
            thumbStream.Position = 0;

            //Save
            thumbUrl = AddFile(thumbStream, "png");

            //Create asset entry
            DeltaAsset asset = new DeltaAsset
            {
                image_url = baseUrl,
                image_thumb_url = thumbUrl
            };

            //Create DB entry
            metadata = new CDbTexture2D
            {
                id = texture.file.GetGamePath(),
                asset = asset,
                sha256 = hash,
                time = DateTime.UtcNow
            };
            persist.db_uploaded_texture.Upsert(metadata);

            return asset;
        }
        
        /// <summary>
        /// Adds a new file and generate a new GUID. Returns it's URI
        /// </summary>
        /// <param name="id"></param>
        /// <param name="data"></param>
        public string AddFile(Stream data, string extension)
        {
            //Generate a GUID for this asset
            Guid guid = GenerateUniqueGuid();

            //Insert this into the transport
            string f = transport.AddFile("/" + guid.ToString() + "." + extension, data);

            //Insert this into the database
            CDbUploadedAsset asset = new CDbUploadedAsset
            {
                id = guid,
                url = f,
                time = DateTime.UtcNow,
                type = -1,
                description = extension
            };
            persist.db_uploaded_assets.Insert(asset);

            return f;
        }

        /// <summary>
        /// Generates a unique GUID
        /// </summary>
        /// <returns></returns>
        private Guid GenerateUniqueGuid()
        {
            Guid g = Guid.NewGuid();
            while (persist.db_uploaded_assets.FindById(g) != null)
                g = Guid.NewGuid();
            return g;
        }
    }
}
