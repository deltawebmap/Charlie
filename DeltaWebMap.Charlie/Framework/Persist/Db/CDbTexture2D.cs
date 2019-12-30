using LibDeltaSystem.Entities.ArkEntries;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeltaWebMap.Charlie.Framework.Persist.Db
{
    /// <summary>
    /// Represents a texture 2D that was uploaded to the server.
    /// </summary>
    public class CDbTexture2D
    {
        /// <summary>
        /// The LiteDB id, but also the package path to this file
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// SHA-256 hash of the file
        /// </summary>
        public byte[] sha256 { get; set; }

        /// <summary>
        /// Time of the last time this file was edited
        /// </summary>
        public DateTime time { get; set; }

        /// <summary>
        /// The asset data
        /// </summary>
        public DeltaAsset asset { get; set; }
    }
}
