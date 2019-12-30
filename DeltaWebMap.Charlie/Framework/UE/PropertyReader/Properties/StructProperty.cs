using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeltaWebMap.Charlie.Framework.UE.Assets;
using DeltaWebMap.Charlie.Framework.UE.PropertyReader.Structs;

namespace DeltaWebMap.Charlie.Framework.UE.PropertyReader.Properties
{
    public class StructProperty : BaseProperty
    {
        public string structType; //Only set if this is not in an array
        public int structTypeIndex; //Only set if this is not in an array
        public BaseStruct value;

        /// <summary>
        /// A list of string names that are known to be prop list structs
        /// </summary>
        public static readonly string[] PROP_LIST_STRUCT_NAMES = new string[]
        {
            "ItemStatInfo",
            "ItemNetID",
            "ItemNetInfo",
            "Transform",
            "PrimalPlayerDataStruct",
            "PrimalPlayerCharacterConfigStruct",
            "PrimalPersistentCharacterStatsStruct",
            "TribeData",
            "TribeGovernment",
            "TerrainInfo",
            "ArkInventoryData",
            "DinoOrderGroup",
            "ARKDinoData",
            "WeightedObjectList",
            "DecalData",
            /* Below are more unsure, but still likely */
            "Anchors",
            "StatusValueModifierDescription",
            "HUDElement",
            "ProjectileArc",
            "MultiUseEntry",
            "BoidBehavior",
            "FlockPersistentData",
            /* Even more */
            "CustomItemByteArrays",
            "CustomItemDoubles",
            "PrimalStructureSnapTagSettings",
            "HordeCrateDifficultyLevel",
            "HordeCrateNPCGroup",
            "DinoSetup",
            "WeaponAnim",
            "CanvasIcon",
            "WeaponData"
        };

        public override string GetDebugString()
        {
            return $"structType={structType}, structTypeIndex={structTypeIndex}, value={{{value.GetDebugString()}}}";
        }

        public override void Link(UPropertyGroup g, UAssetFile file)
        {
            //No linking needed...
        }

        public override void Read(IOMemoryStream ms, UAssetFile file)
        {
            //Only process the struct type if we're not inside an array
            if(!isArray)
            {
                //Read type
                structType = ms.ReadNameTableEntry(file);
                structTypeIndex = ms.ReadInt();

                //Read
                BaseStruct st;
                if (PROP_LIST_STRUCT_NAMES.Contains(structType))
                {
                    //Open this as a struct property list.
                    st = new PropListStruct();
                }
                else if (structType == "Vector" || structType == "Rotator")
                {
                    //3d vector or rotor 
                    st = new Vector3Struct();
                }
                else if (structType == "Vector2D")
                {
                    //2d vector
                    st = new Vector2Struct();
                }
                else if (structType == "Quat")
                {
                    //Quat
                    st = new QuatStruct();
                }
                else if (structType == "Color")
                {
                    //Color
                    st = new ColorStruct();
                }
                else if (structType == "LinearColor")
                {
                    //Linear color
                    st = new LinearColorStruct();
                }
                else if (structType == "UniqueNetIdRepl")
                {
                    //Some net stuff
                    st = new UniqueNetIdStruct();
                }
                else if (structType == "Guid")
                {
                    //Some net stuff
                    st = new GuidStruct();
                }
                else if (structType == "IntPoint")
                {
                    //Some net stuff
                    st = new IntPointStruct();
                }
                else if (structType == "StringAssetReference")
                {
                    st = new StringAssetReferenceStruct();
                }
                else
                {
                    //Interpet this as a struct property list. Maybe raise a warning later?
                    file.Warn("Struct Warning", $"Unknown type '{structType}'. Interpeting as a struct property list...");
                    st = new PropListStruct();
                }

                //Read
                st.ReadStruct(ms, file, this, array);
                value = st;
            } else
            {
                //Try and determine the struct type from the length. Seems very ugly
                //Takes some inspiration from https://github.com/ark-mod/ArkSavegameToolkitNet/blob/master/ArkSavegameToolkitNet/Arrays/ArkArrayStruct.cs
                BaseStruct st;
                if (array.length == array.arrayItemCount * 4 + 4)
                    st = new ColorStruct();
                else if (array.length == array.arrayItemCount * 12 + 4)
                    st = new Vector3Struct();
                else if (array.length == array.arrayItemCount * 16 + 4)
                    st = new LinearColorStruct();
                else
                    st = new PropListStruct(); //Assume this is a list

                //Read
                st.ReadStruct(ms, file, this, array);
                value = st;
            }
        }
    }
}
