using DeltaWebMap.Charlie.Framework.UE.Assets;
using DeltaWebMap.Charlie.Framework.UE.PropertyReader.Properties;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeltaWebMap.Charlie.Framework.UE.PropertyReader
{
    public abstract class BaseProperty
    {
        public string name;
        public int nameIndex;
        public string type;
        public int typeIndex;
        public int length;
        public int index;

        public bool isArray;
        public ArrayProperty array;

        public long contentBeginPosition;

        public abstract void Read(IOMemoryStream ms, UAssetFile file);
        public abstract string GetDebugString();
        public abstract void Link(UPropertyGroup g, UAssetFile file);

        public static BaseProperty ReadProperty(IOMemoryStream ms, UAssetFile f, ArrayProperty array = null)
        {
            //Define headers
            string name;
            int nameIndex;
            string type;
            int typeIndex;
            
            //If this is an array, pull from the array. If it isn't, pull from the file
            if(array == null)
            {
                //Read name and type so that we can determine the property type
                name = ms.ReadNameTableEntry(f);
                nameIndex = ms.ReadInt(); //CHECK: This might come after the "None" check, but I don't know that as certain.
                if (name == "None")
                    return null;

                //Read type
                type = ms.ReadNameTableEntry(f);
                typeIndex = ms.ReadInt();
            } else
            {
                name = null;
                nameIndex = 0;
                type = array.arrayFieldType;
                typeIndex = array.arrayFieldTypeIndex;
            }

            //Generate the correct property type
            BaseProperty prop;
            switch(type)
            {
                case "ArrayProperty": prop = new ArrayProperty(); break;
                case "BoolProperty": prop = new BoolProperty(); break;
                case "ByteProperty": prop = new ByteProperty(); break;
                case "DoubleProperty": prop = new DoubleProperty(); break;
                case "FloatProperty": prop = new FloatProperty(); break;
                case "Int16Property": prop = new Int16Property(); break;
                case "Int8Property": prop = new Int8Property(); break;
                case "IntProperty": prop = new IntProperty(); break;
                case "NameProperty": prop = new NameProperty(); break;
                case "ObjectProperty": prop = new ObjectProperty(); break;
                case "StrProperty": prop = new StrProperty(); break;
                case "StructProperty": prop = new StructProperty(); break;
                case "TextProperty": prop = new TextProperty(); break;
                case "UInt16Property": prop = new UInt16Property(); break;
                case "UInt32Property": prop = new UInt32Property(); break;
                case "UInt64Property": prop = new UInt64Property(); break;
                default:
                    throw new Exception($"Unexpected and invalid type {type}:{typeIndex} read for property with name {name}:{nameIndex}. Aborting!");
            }

            //Set all of the values we already read
            prop.name = name;
            prop.nameIndex = nameIndex;
            prop.type = type;
            prop.typeIndex = typeIndex;

            //Now, read the remaining properties
            if(array == null)
            {
                prop.length = ms.ReadInt();
                prop.index = ms.ReadInt();
            }

            //Now, read the prop
            prop.contentBeginPosition = ms.position;
            prop.isArray = array != null;
            prop.array = array;
            prop.Read(ms, f);

            //Return this deserialized property
            return prop;
        }
    }
}
