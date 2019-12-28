using DeltaWebMap.Charlie.Framework.Exceptions;
using DeltaWebMap.Charlie.Framework.UE.PropertyReader;
using DeltaWebMap.Charlie.Framework.UE.PropertyReader.Properties;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeltaWebMap.Charlie.Framework.UE.Assets.UAssetTypes
{
    public class UAssetBlueprint : UAssetFile
    {
        /// <summary>
        /// Reference to the parent class file. If this is null, this does not have a Blueprint child.
        /// </summary>
        public UENamespaceFile parentFile;

        /// <summary>
        /// My own defaults. Does NOT read parent values.
        /// </summary>
        public UPropertyGroup myDefaults;

        /// <summary>
        /// Defaults of this, respecting parents.
        /// </summary>
        public UPropertyGroup defaults;

        /// <summary>
        /// Contains all of the components added in the "components" tab in the ADK
        /// </summary>
        public List<UENamespaceFile> components;

        /// <summary>
        /// The construction script head.
        /// </summary>
        public EmbeddedGameObjectTableHead constructor;

        public override void BaseReadFile(UEInstall install, string path)
        {
            base.BaseReadFile(install, path);

            //Set metdata
            ReadMetadata();

            //Read my defaults
            ReadMyDefaults();

            //Read the construction script
            if(constructor != null)
                ReadConstructionScript();

            //Read parent, if we have one
            if (parentFile != null)
            {
                //Read parent
                UAssetBlueprint parent = GetParentBlueprint();

                //Now, copy the defaults and override ones that we override
                defaults = parent.defaults.GetCopy();

                //Override the defaults in here that myDefaults has
                foreach(var d in myDefaults.props)
                {
                    BaseProperty dToReplace = null;

                    //Find
                    foreach (var dd in defaults.props)
                    {
                        if(dd.name == d.name && dd.nameIndex == d.nameIndex)
                        {
                            dToReplace = dd;
                        }
                    }

                    //Replace
                    defaults.props.Remove(dToReplace);
                    defaults.props.Add(d);
                }
            } else
            {
                defaults = myDefaults;
            }
        }

        /// <summary>
        /// Reads the parent Blueprint file of this, or pulls it from the cache if we can.
        /// </summary>
        /// <returns></returns>
        public UAssetBlueprint GetParentBlueprint()
        {
            return install.OpenBlueprint(parentFile.GetFilename());
        }

        private void ReadMetadata()
        {
            //Get the metadata head
            var metadataHead = GetMetadataHead();

            //Now, read and decode the metadata head
            var metadata = ReadUPropertyGroupFromObject(metadataHead);

            //Get the construction script from this
            if (metadata.HasProperty("SimpleConstructionScript"))
                constructor = metadata.GetPropertyByName<ObjectProperty>("SimpleConstructionScript").GetEmbeddedReferencedHead(this);
            else
                constructor = null;

            //Get the parent class file and follow to get the original, containing the class name
            var parentHead = metadata.GetPropertyByName<ObjectProperty>("ParentClass").GetReferencedHead(this).GetUnderlyingHead(this);

            //The parent file might be a C++ class, which we can't read. If that's the case, just set the parent to null.
            if (parentHead.IsValidFile())
                parentFile = parentHead.GetReferencedFile(this);
            else
                parentFile = null;
        }

        private void ReadMyDefaults()
        {
            //Get the name of the default header
            string defaultHeaderName = "Default__" + GetMetadataHead().type + "_C";

            //Find
            var head = GetEmbedByTypeName(defaultHeaderName);
            if (head == null)
                throw new FailedToFindDefaultsException();

            //Decode and read
            myDefaults = ReadUPropertyGroupFromObject(head);
        }

        private EmbeddedGameObjectTableHead GetMetadataHead()
        {
            //Find the metadata item. There should be ONE file with unknown5 == 11 that defines this
            EmbeddedGameObjectTableHead h = null;
            foreach (var e in gameObjectEmbeds)
            {
                if (e.unknown5 == 11 && h == null)
                    h = e;
                else if (e.unknown5 == 11)
                    throw new Exception("Multiple objects tagged for metadata found for Blueprint! Something is wrong.");
            }
            if (h == null)
                throw new Exception("No objects tagged for metadata found for Blueprint! Something is wrong.");
            return h;
        }

        private void ReadConstructionScript()
        {
            //Read
            var construction = ReadUPropertyGroupFromObject(constructor);

            //Set components to a new list for the next step
            components = new List<UENamespaceFile>();

            //Get RootNodes. We usually just use this for getting a dino's status component
            if (construction.HasProperty("RootNodes"))
            {
                var roots = construction.GetPropertyByName<ArrayProperty>("RootNodes");
                foreach (var r in roots.properties)
                {
                    //Get the node info
                    var ehead = ((ObjectProperty)r).GetEmbeddedReferencedHead(this);
                    var data = ReadUPropertyGroupFromObject(ehead);

                    //Use the node info to get the template
                    if (!data.HasProperty("ComponentTemplate"))
                        continue;
                    var templateHead = data.GetPropertyByName<ObjectProperty>("ComponentTemplate").GetEmbeddedReferencedHead(this);

                    //Use the template ID to get the referenced GameObject. Then, get the underlying header
                    var templateData = gameObjectReferences[(-templateHead.id) - 1].GetUnderlyingHead(this);

                    //Work up to get the game path. If this isn't a valid file, we know this is not the correct one
                    if (!templateData.IsValidFile())
                        continue;
                    var templateDataFullname = templateData.GetReferencedFile(this);

                    //We now have the fullname of this component. Add it to the list of components
                    components.Add(templateDataFullname);
                }
            }
        }
    }
}
