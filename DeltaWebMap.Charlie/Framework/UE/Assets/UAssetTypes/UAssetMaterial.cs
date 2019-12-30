using DeltaWebMap.Charlie.Framework.UE.PropertyReader;
using DeltaWebMap.Charlie.Framework.UE.PropertyReader.Properties;
using DeltaWebMap.Charlie.Framework.UE.PropertyReader.Structs;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeltaWebMap.Charlie.Framework.UE.Assets.UAssetTypes
{
    public class UAssetMaterial : UAssetFile
    {
        /// <summary>
        /// Holds material properties
        /// </summary>
        public UPropertyGroup properties;

        /// <summary>
        /// Holds texture params
        /// </summary>
        public List<TextureParameterValue> textureParameters;

        public override void BaseReadFile(UEInstall install, string path)
        {
            base.BaseReadFile(install, path);

            //Read properties
            OpenProperties();

            //Open params
            ConvertTextureParameters();
        }

        void OpenProperties()
        {
            //The properties have the same name as we do
            EmbeddedGameObjectTableHead h = GetEmbedByTypeName(classname);
            UPropertyGroup group = ReadUPropertyGroupFromObject(h);
            properties = group;
        }

        void ConvertTextureParameters()
        {
            ArrayProperty p = properties.GetPropertyByName<ArrayProperty>("TextureParameterValues");
            textureParameters = new List<TextureParameterValue>();
            foreach (var e in p.properties)
            {
                StructProperty sp = (StructProperty)e;
                PropListStruct lp = (PropListStruct)sp.value;
                textureParameters.Add(new TextureParameterValue
                {
                    name = lp.properties.GetPropertyName("ParameterName", null),
                    prop = lp.properties.GetPropertyByName<ObjectProperty>("ParameterValue")
                });
            }
        }

        public class TextureParameterValue
        {
            public string name;
            public ObjectProperty prop;
        }
    }
}
