using DeltaWebMap.Charlie.Framework.Exceptions;
using DeltaWebMap.Charlie.Framework.UE.Assets;
using DeltaWebMap.Charlie.Framework.UE.PropertyReader.Properties;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeltaWebMap.Charlie.Framework.UE.PropertyReader
{
    public class UPropertyGroup
    {
        public List<BaseProperty> props;

        public void ReadProps(IOMemoryStream ms, UAssetFile f, ArrayProperty s = null)
        {
            //Create props list
            props = new List<BaseProperty>();
            
            //Read until None, which will return null
            BaseProperty p = BaseProperty.ReadProperty(ms, f, s);
            while(p != null)
            {
                props.Add(p);
                p = BaseProperty.ReadProperty(ms, f, s);
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

        public string GetPropertyString(string name, string defaultValue)
        {
            if (!HasProperty(name) && defaultValue != null)
                return defaultValue;
            else if (!HasProperty(name))
                throw new PropertyNotFoundException();
            StrProperty p = GetPropertyByName<StrProperty>(name);
            return p.data;
        }

        public float GetPropertyFloat(string name, float? defaultValue, int index = 0)
        {
            if (!HasProperty(name) && defaultValue != null)
                return defaultValue.Value;
            else if (!HasProperty(name))
                throw new PropertyNotFoundException();
            FloatProperty p = GetPropertyByName<FloatProperty>(name, index);
            return p.value;
        }

        public int GetPropertyInt(string name, int? defaultValue, int index = 0)
        {
            if (!HasProperty(name) && defaultValue != null)
                return defaultValue.Value;
            else if (!HasProperty(name))
                throw new PropertyNotFoundException();
            IntProperty p = GetPropertyByName<IntProperty>(name, index);
            return p.value;
        }

        public bool GetPropertyBool(string name, bool? defaultValue)
        {
            if (!HasProperty(name) && defaultValue != null)
                return defaultValue.Value;
            else if (!HasProperty(name))
                throw new PropertyNotFoundException();
            BoolProperty p = GetPropertyByName<BoolProperty>(name);
            return p.value;
        }

        public string GetPropertyName(string name, string defaultValue)
        {
            if (!HasProperty(name) && defaultValue != null)
                return defaultValue;
            else if (!HasProperty(name))
                throw new PropertyNotFoundException();
            NameProperty p = GetPropertyByName<NameProperty>(name);
            return p.valueName;
        }
    }
}
