using System;
using System.Collections.Generic;
using System.Text;

namespace DeltaWebMap.Charlie.Framework
{
    public class CharlieConfig
    {
        public string[] exclude_regex;

        public string assets_url_host;
        public string firebase_project_id;

        public string persist;
        public string firebase_cfg;
        public string delta_cfg;
        public string temp;
    }
}
