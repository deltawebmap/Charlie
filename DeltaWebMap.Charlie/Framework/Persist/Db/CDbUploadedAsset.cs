using System;
using System.Collections.Generic;
using System.Text;

namespace DeltaWebMap.Charlie.Framework.Persist.Db
{
    public class CDbUploadedAsset
    {
        public Guid id { get; set; }
        public DateTime time { get; set; }
        public int type { get; set; }
        public string description { get; set; }
        public string url { get; set; }
    }
}
