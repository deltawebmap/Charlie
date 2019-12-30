using System;
using System.Collections.Generic;
using System.Text;

namespace DeltaWebMap.Charlie.Framework.Firebase.FirebaseCreateVersionRequest
{
    public class Header
    {
        public string glob { get; set; }
        public Dictionary<string, string> headers { get; set; }
    }

    public class Config
    {
        public List<Header> headers { get; set; }
    }

    public class FirebaseCreateVersionRequest
    {
        public Config config { get; set; }
    }

}
