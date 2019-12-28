using DeltaWebMap.Charlie.Framework.UE.Assets;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeltaWebMap.Charlie.Framework.UE.PropertyReader
{
    public class UPropertyGroup
    {
        public List<BaseProperty> props;

        public void ReadProps(IOMemoryStream ms, UAssetFile f)
        {
            //Create props list
            props = new List<BaseProperty>();
            
            //Read until None, which will return null
            BaseProperty p = BaseProperty.ReadProperty(ms, f, null);
            while(p != null)
            {
                props.Add(p);
                p = BaseProperty.ReadProperty(ms, f, null);
            }

            //Now, link all of the properties
            foreach(var prop in props)
            {
                prop.Link(this, f);
            }
        }

        public UPropertyGroup GetCopy()
        {
            return new UPropertyGroup
            {
                props = new List<BaseProperty>(props)
            };
        }

        public BaseProperty GetPropertyByName(string name, int index = 0)
        {
            foreach(var p in props)
            {
                if (p.name == name && p.index == index)
                    return p;
            }
            return null;
        }

        public bool HasProperty(string name, int index = 0)
        {
            return GetPropertyByName(name, index) != null;
        }

        public T GetPropertyByName<T>(string name, int index = 0)
        {
            //Read property like normal
            BaseProperty p = GetPropertyByName(name, index);

            //Check if it is null
            if (p == null)
                return default(T);

            //Make sure it's type matches
            if (typeof(T) != p.GetType())
                throw new Exception($"Attempted to read {p.GetType().FullName} at {typeof(T).FullName}!");

            //Return it
            return (T)Convert.ChangeType(p, typeof(T));
        }
    }
}
