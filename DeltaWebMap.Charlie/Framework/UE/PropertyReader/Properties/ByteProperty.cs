using System;
using System.Collections.Generic;
using System.Text;
using DeltaWebMap.Charlie.Framework.UE.Assets;

namespace DeltaWebMap.Charlie.Framework.UE.PropertyReader.Properties
{
    public class ByteProperty : BaseProperty
    {
        public string enumName;

        public bool isNormalByte; //If this is true, this is just a normal byte. If it is not true, use the ClassName instead

        //=== VALUES ===
        public string enumValue; //Use ONLY if the above boolean is false
        public string enumType; //Use ONLY if the above boolean is false
        public byte byteValue; //Use ONLY if the above boolean is true

        public override string GetDebugString()
        {
            if (enumName == null)
                return $"isNormalByte={isNormalByte.ToString()}, enumName=, byteValue={byteValue.ToString()}, enumValue={enumValue}, enumType={enumType}";
            else
                return $"isNormalByte={isNormalByte.ToString()}, enumName={enumName.ToString()}, byteValue={byteValue.ToString()}, enumValue={enumValue}, enumType={enumType}";
        }

        public override void Link(UPropertyGroup g, UAssetFile file)
        {
            //Don't need to link....
        }

        public override void Read(IOMemoryStream ms, UAssetFile file)
        {
            if (length == 1 || isArray) /* Unsure if isArray is valid here. It threw an error on length = 0 before. */
            {
                //Read in the enum name
                enumName = ms.ReadNameTableEntry(file);

                //That can be None, but cannot be null.
                if (enumName == null)
                    throw new Exception("Tried to read enum type, but got null!");

                isNormalByte = enumName == "None";

                //If that type is a None, this is not an enum. If it is, this is an enum. Read the name.
                if (isNormalByte)
                {
                    byteValue = ms.ReadByte();
                    ms.ReadInt();
                }
                else
                    enumValue = ms.ReadNameTableEntry(file);
            }
            else if (length == 8)
            {
                //If the length is 8, this is an enum. It seems to follow like this...
                //Enum name
                //Int, usually 0
                //Enum value
                //Int, usually 0
                enumType = ms.ReadNameTableEntry(file);
                ms.position += 4;
                enumValue = ms.ReadNameTableEntry(file);
                ms.position += 4;
            }
            else
            {
                throw new Exception($"Warning: Unknown ByteProperty length '{length}'.");
            }
        }
    }
}
