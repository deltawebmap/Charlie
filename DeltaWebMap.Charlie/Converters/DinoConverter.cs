using DeltaWebMap.Charlie.Framework;
using DeltaWebMap.Charlie.Framework.UE.Assets.UAssetTypes;
using DeltaWebMap.Charlie.Framework.UE.PropertyReader.Properties;
using DeltaWebMap.Charlie.Framework.UE.PropertyReader.Structs;
using LibDeltaSystem.Entities.ArkEntries;
using LibDeltaSystem.Entities.ArkEntries.Dinosaur;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeltaWebMap.Charlie.Converters
{
    public static class DinoConverter
    {
        public static DinosaurEntry ConvertDino(CharlieSession session, UAssetBlueprint bp)
        {
            //Get status component
            UAssetBlueprint status = GetStatusComponent(bp);
            if (status == null)
                return null;

            //Get dino settings classes
            UAssetBlueprint adultSettings = GetSettingsBlueprint(bp, "AdultDinoSettings");
            UAssetBlueprint childSettings = GetSettingsBlueprint(bp, "BabyDinoSettings");
            if (adultSettings == null)
                adultSettings = GetSettingsBlueprint(bp, "DinoSettingsClass");
            if (childSettings == null)
                childSettings = GetSettingsBlueprint(bp, "DinoSettingsClass");
            if (adultSettings == null || childSettings == null)
                return null;

            //Use name tag to find entry
            string tag = bp.defaults.GetPropertyName("DinoNameTag", null);
            if (!session.dinoEntries.ContainsKey(tag))
                return null;
            UAssetBlueprint entry = session.dinoEntries[tag];

            //Get the icon
            var entryMaterial = entry.defaults.GetPropertyByName<ObjectProperty>("DinoMaterial").GetReferencedMaterialAsset();
            var icon = entryMaterial.textureParameters[0].prop.GetReferencedTexture2DAsset();
            DeltaAsset iconAsset = session.assetManager.AddTexture2D(icon);

            //Read and convert it
            DinosaurEntry e = new DinosaurEntry
            {
                screen_name = bp.defaults.GetPropertyString("DescriptiveName", null),
                colorizationIntensity = bp.defaults.GetPropertyFloat("ColorizationIntensity", 1),
                babyGestationSpeed = bp.defaults.GetPropertyFloat("BabyGestationSpeed", -1),
                extraBabyGestationSpeedMultiplier = bp.defaults.GetPropertyFloat("ExtraBabyGestationSpeedMultiplier", -1),
                babyAgeSpeed = bp.defaults.GetPropertyFloat("BabyAgeSpeed", null),
                extraBabyAgeMultiplier = bp.defaults.GetPropertyFloat("ExtraBabyAgeSpeedMultiplier", -1),
                useBabyGestation = bp.defaults.GetPropertyBool("bUseBabyGestation", false),
                statusComponent = ConvertStatus(status),
                adultFoods = ConvertFoods(adultSettings),
                childFoods = ConvertFoods(childSettings),
                classname = bp.classname,
                icon = iconAsset
            };

            //Finally, read stats
            RipStats(bp, e);
            return e;
        }

        private static UAssetBlueprint GetStatusComponent(UAssetBlueprint bp)
        {
            //Try to find the status component, and also check parents
            while(bp != null)
            {
                //Loop through components to find the status component
                foreach (var c in bp.components)
                {
                    UAssetBlueprint component = bp.install.OpenBlueprint(c.GetFilename());
                    bool isStatus = component.GetUnderlyingParent().classname == "DinoCharacterStatusComponent_BP";
                    if (isStatus)
                        return component;
                }

                //Get parent
                bp = bp.parentClass;
            }
            return null;
        }

        public static DinosaurEntryStatusComponent ConvertStatus(UAssetBlueprint bp)
        {
            return new DinosaurEntryStatusComponent
            {
                baseFoodConsumptionRate = bp.defaults.GetPropertyFloat("BaseFoodConsumptionRate", null),
                babyDinoConsumingFoodRateMultiplier = bp.defaults.GetPropertyFloat("BabyDinoConsumingFoodRateMultiplier", 25.5f),
                extraBabyDinoConsumingFoodRateMultiplier = bp.defaults.GetPropertyFloat("ExtraBabyDinoConsumingFoodRateMultiplier", 20),
                foodConsumptionMultiplier = bp.defaults.GetPropertyFloat("FoodConsumptionMultiplier", 1),
                tamedBaseHealthMultiplier = bp.defaults.GetPropertyFloat("TamedBaseHealthMultiplier", 1)
            };
        }

        private static UAssetBlueprint GetSettingsBlueprint(UAssetBlueprint dino, string name)
        {
            //Get property reference
            ObjectProperty p = dino.defaults.GetPropertyByName<ObjectProperty>(name);
            if (p == null)
                return null;

            //Check validity
            if (!p.GetIsValid())
                return null;

            //Get referenced file
            return p.GetReferencedBlueprintAsset();
        }

        private static List<DinosaurEntryFood> ConvertFoods(UAssetBlueprint settings)
        {
            List<DinosaurEntryFood> output = new List<DinosaurEntryFood>();

            //Get each
            ArrayProperty mBase = settings.defaults.GetPropertyByName<ArrayProperty>("FoodEffectivenessMultipliers");
            ArrayProperty mExtra = settings.defaults.GetPropertyByName<ArrayProperty>("ExtraFoodEffectivenessMultipliers");

            //Convert
            if (mBase != null)
                output.AddRange(ConvertMultiplier(settings, mBase));
            if (mExtra != null)
                output.AddRange(ConvertMultiplier(settings, mExtra));

            return output;
        }

        private static List<DinosaurEntryFood> ConvertMultiplier(UAssetBlueprint settings, ArrayProperty p)
        {
            //Convert each entry
            List<DinosaurEntryFood> output = new List<DinosaurEntryFood>();
            foreach (var s in p.properties)
            {
                StructProperty data = (StructProperty)s;
                PropListStruct sdata = (PropListStruct)data.value;
                UAssetBlueprint foodClass = sdata.properties.GetPropertyByName<ObjectProperty>("FoodItemParent").GetReferencedBlueprintAsset();
                DinosaurEntryFood food = new DinosaurEntryFood
                {
                    classname = foodClass.classname,
                    foodEffectivenessMultiplier = sdata.properties.GetPropertyFloat("FoodEffectivenessMultiplier", null),
                    affinityOverride = sdata.properties.GetPropertyFloat("AffinityOverride", null),
                    affinityEffectivenessMultiplier = sdata.properties.GetPropertyFloat("AffinityEffectivenessMultiplier", null),
                    foodCategory = sdata.properties.GetPropertyInt("FoodItemCategory", null),
                    priority = sdata.properties.GetPropertyFloat("UntamedFoodConsumptionPriority", null)
                };
                output.Add(food);
            }
            return output;
        }

        private static void RipStats(UAssetBlueprint status, DinosaurEntry entry)
        {
            //Create arrays
            entry.baseLevel = new float[12];
            entry.increasePerWildLevel = new float[12];
            entry.increasePerTamedLevel = new float[12];
            entry.additiveTamingBonus = new float[12];
            entry.multiplicativeTamingBonus = new float[12];

            //Loop through ARK indexes
            for (int i = 0; i <= 11; i++)
            {
                //Calculate multipliers
                bool can_level = true;// (i == 2) || (reader.GetPropertyByte("CanLevelUpValue", CANLEVELUP_VALUES[i], i) == 1);
                int add_one = IS_PERCENT_STAT[i];
                float zero_mult = can_level ? 1 : 0;
                float ETHM = status.defaults.GetPropertyFloat("ExtraTamedHealthMultiplier", EXTRA_MULTS_VALUES[i], i);

                //Add stat data
                entry.baseLevel[i] = MathF.Round(status.defaults.GetPropertyFloat("MaxStatusValues", BASE_VALUES[i], i) + add_one, ROUND_PERCISION);
                entry.increasePerWildLevel[i] = MathF.Round(status.defaults.GetPropertyFloat("AmountMaxGainedPerLevelUpValue", IW_VALUES[i], i) * zero_mult, ROUND_PERCISION);
                entry.increasePerTamedLevel[i] = MathF.Round(status.defaults.GetPropertyFloat("AmountMaxGainedPerLevelUpValueTamed", 0, i) * ETHM * zero_mult, ROUND_PERCISION);
                entry.additiveTamingBonus[i] = MathF.Round(status.defaults.GetPropertyFloat("TamingMaxStatAdditions", 0, i), ROUND_PERCISION);
                entry.multiplicativeTamingBonus[i] = MathF.Round(status.defaults.GetPropertyFloat("TamingMaxStatMultipliers", 0, i), ROUND_PERCISION);
            }
        }

        public const int ROUND_PERCISION = 6;

        /* New defaults */
        //https://github.com/arkutils/Purlovia/blob/f25dd80a06930f0d34beacd03dafc5f9cecb054e/ark/defaults.py
        public const float FEMALE_MINTIMEBETWEENMATING_DEFAULT = 64800.0f;
        public const float FEMALE_MAXTIMEBETWEENMATING_DEFAULT = 172800.0f;

        public const float BABYGESTATIONSPEED_DEFAULT = 0.000035f;

        public static readonly float[] BASE_VALUES = new float[] { 100, 100, 100, 100, 100, 100, 0, 0, 0, 0, 0, 0 };
        public static readonly float[] IW_VALUES = new float[] { 0, 0, 0.06f, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        public static readonly float[] IMPRINT_VALUES = new float[] { 0.2f, 0, 0.2f, 0, 0.2f, 0.2f, 0, 0.2f, 0.2f, 0.2f, 0, 0 };
        public static readonly float[] EXTRA_MULTS_VALUES = new float[] { 1.35f, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
        public static readonly float[] DONTUSESTAT_VALUES = new float[] { 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 1, 1 };
        public static readonly byte[] CANLEVELUP_VALUES = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        public static readonly int[] IS_PERCENT_STAT = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 1 };
    }
}
