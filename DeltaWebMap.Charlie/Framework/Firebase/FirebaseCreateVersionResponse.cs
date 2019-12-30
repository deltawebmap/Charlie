using System;
using System.Collections.Generic;
using System.Text;

namespace DeltaWebMap.Charlie.Framework.Firebase.FirebaseCreateVersionResponse
{
    public class Header
    {
        public Dictionary<string, string> headers { get; set; }
        public string glob { get; set; }
    }

    public class Config
    {
        public List<Header> headers { get; set; }
    }

    public class FirebaseCreateVersionResponse
    {
        public string name { get; set; }
        public string status { get; set; }
        public Config config { get; set; }
    }
}
