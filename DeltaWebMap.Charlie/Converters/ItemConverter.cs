using DeltaWebMap.Charlie.Framework;
using DeltaWebMap.Charlie.Framework.UE.Assets.UAssetTypes;
using DeltaWebMap.Charlie.Framework.UE.PropertyReader;
using DeltaWebMap.Charlie.Framework.UE.PropertyReader.Properties;
using DeltaWebMap.Charlie.Framework.UE.PropertyReader.Structs;
using LibDeltaSystem.Entities.ArkEntries;
using LibDeltaSystem.Entities.ArkEntries.Dinosaur;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeltaWebMap.Charlie.Converters
{
    public static class ItemConverter
    {
        public static ItemEntry ConvertItem(CharlieSession session, UAssetBlueprint bp)
        {
            //Get the primary icon
            DeltaAsset iconAsset;
            try
            {
                ObjectProperty iconRef = bp.defaults.GetPropertyByName<ObjectProperty>("ItemIcon");
                UAssetTexture2D icon = iconRef.GetReferencedTexture2DAsset();
                iconAsset = session.assetManager.AddTexture2D(icon);
            } catch (Exception ex)
            {
                //Fallback
                session.Log("ItemConverter", "Failed to get item for " + bp.pathname, ConsoleColor.Yellow);
                iconAsset = DeltaAsset.MISSING_ICON;
            }

            //Get the array of UseItemAddCharacterStatusValues
            ArrayProperty statusValuesArray = bp.defaults.GetPropertyByName<ArrayProperty>("UseItemAddCharacterStatusValues");
            List<ItemEntry_ConsumableAddStatusValue> statusValues = new List<ItemEntry_ConsumableAddStatusValue>();
            if (statusValuesArray != null)
            {
                foreach (var i in statusValuesArray.properties)
                {
                    StructProperty sv = (StructProperty)i;
                    var svp = ((PropListStruct)sv.value).properties;
                    string type = svp.GetPropertyByName<ByteProperty>("StatusValueType").enumValue;
                    ItemEntry_ConsumableAddStatusValue sve = ConvertAddValues(svp, type);
                    statusValues.Add(sve);
                }
            }

            //If this is for a structure, get the structure to build
            var structureRef = bp.defaults.GetPropertyByName<ObjectProperty>("StructureToBuild");
            string structureName = null;
            if(structureRef != null)
            {
                var structure = structureRef.GetReferencedBlueprintAsset();
                structureName = structure.classname;
                //We'll likely add more here later
            }

            //Create
            ItemEntry e = new ItemEntry
            {
                hideFromInventoryDisplay = bp.defaults.GetPropertyBool("bHideFromInventoryDisplay", false),
                useItemDurability = bp.defaults.GetPropertyBool("bUseItemDurability", false),
                isTekItem = bp.defaults.GetPropertyBool("bTekItem", false),
                allowUseWhileRiding = bp.defaults.GetPropertyBool("bAllowUseWhileRiding", false),
                name = bp.defaults.GetPropertyString("DescriptiveNameBase", null),
                description = bp.defaults.GetPropertyString("ItemDescription", null),
                spoilingTime = bp.defaults.GetPropertyFloat("SpolingTime", 0),
                baseItemWeight = bp.defaults.GetPropertyFloat("BaseItemWeight", 0),
                useCooldownTime = bp.defaults.GetPropertyFloat("MinimumUseInterval", 0),
                baseCraftingXP = bp.defaults.GetPropertyFloat("BaseCraftingXP", 0),
                baseRepairingXP = bp.defaults.GetPropertyFloat("BaseRepairingXP", 0),
                maxItemQuantity = bp.defaults.GetPropertyInt("MaxItemQuantity", 0),
                classname = bp.classname,
                icon = iconAsset,
                addStatusValues = statusValues.ToArray(),
                structure_classname = structureName,
                hash = 0
            };

            //Hash
            e.hash = ObjectHashTool.HashObject(e);
            return e;
        }

        private static ItemEntry_ConsumableAddStatusValue ConvertAddValues(UPropertyGroup reader, string type)
        {
            return new ItemEntry_ConsumableAddStatusValue
            {
                baseAmountToAdd = reader.GetPropertyFloat("BaseAmountToAdd", null),
                percentOfMaxStatusValue = reader.GetPropertyBool("bPercentOfMaxStatusValue", null),
                percentOfCurrentStatusValue = reader.GetPropertyBool("bPercentOfCurrentStatusValue", null),
                useItemQuality = reader.GetPropertyBool("bUseItemQuality", null),
                addOverTime = reader.GetPropertyBool("bAddOverTime", null),
                setValue = reader.GetPropertyBool("bSetValue", null),
                setAdditionalValue = reader.GetPropertyBool("bSetAdditionalValue", null),
                addOverTimeSpeed = reader.GetPropertyFloat("AddOverTimeSpeed", null),
                itemQualityAddValueMultiplier = reader.GetPropertyFloat("ItemQualityAddValueMultiplier", null),
                statusValueType = type
            };
        }
    }
}
