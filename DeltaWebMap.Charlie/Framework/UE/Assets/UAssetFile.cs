using DeltaWebMap.Charlie.Framework.UE.PropertyReader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DeltaWebMap.Charlie.Framework.UE.Assets
{
    public class UAssetFile
    {
        public IOMemoryStream stream;

        //Package headers
        public int headerUnknown1;
        public int headerUnknown2;
        public int headerUnknown3;
        public int headerUnknown4;
        public int headerUnknown5;
        public int headerUnknown6;
        public int headerUnknown7;
        public string headerUnknown9;
        public int headerUnknown10;
        public int nameTableLength; //How many elements are in the name table
        public int nameTableOffset; //Where the name table is, starting at the beginning of the file
        public int embeddedGameObjectCount; //Number of objects inside of this
        public int headerUnknown11;
        public int refGameObjectCount; //Number of refrenced game objects
        public int headerUnknown12;
        public int binaryIdTableOffset; //Offset to the "binary id" table, st arting from the beginning of the file
        public int headerUnknown13;
        public int thumbnailOffset; //Offset to the thumbnail, starting at the beginning of the file
        public int packagePropertyDictOffset; //Offset to the package property dict
        //(...)
        public int headerPayloadBegin; //Value that seems to be important to reading images. At position 147 in header

        //Package data
        public string[] name_table; //Maps IDs to classnames
        public GameObjectTableHead[] gameObjectReferences;
        public EmbeddedGameObjectTableHead[] gameObjectEmbeds;
        public UEInstall install;

        public virtual void BaseReadFile(UEInstall install, string path)
        {
            //Set
            this.install = install;
            
            //Create a stream
            stream = new IOMemoryStream(new FileStream(path, FileMode.Open, FileAccess.Read), true);

            //Read header data
            ReadHeaderData();

            //Read name table
            ReadNameTable();

            //Read GameObject headers
            ReadGameObjectReferences();

            //Now, read embedded GameObject headers
            ReadEmbeddedGameObjectReferences();
        }

        public void Warn(string topic, string msg)
        {
            Console.WriteLine($"WARNING {topic}: {msg}");
        }

        public EmbeddedGameObjectTableHead GetEmbedByTypeName(string name)
        {
            foreach(var e in gameObjectEmbeds)
            {
                if (e.type == name)
                    return e;
            }
            return null;
        }

        public UPropertyGroup ReadUPropertyGroupFromObject(EmbeddedGameObjectTableHead h)
        {
            UPropertyGroup g = new UPropertyGroup();
            stream.position = h.dataLocation;
            g.ReadProps(stream, this);
            return g;
        }

        /// <summary>
        /// Reads the data at the beginning of the file
        /// </summary>
        void ReadHeaderData()
        {
            stream.position = 0;

            //Start reading header data
            headerUnknown1 = stream.ReadInt();
            headerUnknown2 = stream.ReadInt();
            headerUnknown3 = stream.ReadInt();
            headerUnknown4 = stream.ReadInt();
            headerUnknown5 = stream.ReadInt();
            headerUnknown6 = stream.ReadInt();
            headerUnknown7 = stream.ReadInt();
            headerUnknown9 = stream.ReadUEString();
            headerUnknown10 = stream.ReadInt();
            nameTableLength = stream.ReadInt();
            nameTableOffset = stream.ReadInt();
            embeddedGameObjectCount = stream.ReadInt();
            headerUnknown11 = stream.ReadInt();
            refGameObjectCount = stream.ReadInt();
            headerUnknown12 = stream.ReadInt();
            binaryIdTableOffset = stream.ReadInt();
            headerUnknown13 = stream.ReadInt();
            thumbnailOffset = stream.ReadInt();
            packagePropertyDictOffset = stream.ReadInt();

            //There's some data that we don't know how to get to that seems to sit at 16 bytes before the name table. Read it
            stream.position = nameTableOffset - 16;
            headerPayloadBegin = stream.ReadInt();
        }

        void ReadNameTable()
        {
            //Jump to the beginning of the name table
            stream.position = nameTableOffset;

            //Read array
            name_table = stream.ReadStringArray(nameTableLength);
        }

        void ReadGameObjectReferences()
        {
            //Starts directly after the name table. Assume we're already there
            gameObjectReferences = new GameObjectTableHead[refGameObjectCount];
            for (int i = 0; i < refGameObjectCount; i++)
            {
                GameObjectTableHead h = GameObjectTableHead.ReadEntry(stream, this);
                gameObjectReferences[i] = h;
            }
        }

        void ReadEmbeddedGameObjectReferences()
        {
            //Starts directly after the referenced GameObject table. Assume we're already there
            gameObjectEmbeds = new EmbeddedGameObjectTableHead[embeddedGameObjectCount];
            for (int i = 0; i < embeddedGameObjectCount; i++)
            {
                EmbeddedGameObjectTableHead h = EmbeddedGameObjectTableHead.ReadEntry(stream, this);
                gameObjectEmbeds[i] = h;
            }
        }

        public void DebugWrite()
        {
            var f = this;
            Console.ForegroundColor = ConsoleColor.Red;
            for (int i = 0; i < f.name_table.Length; i++)
                Console.WriteLine($"{i}: {f.name_table[i]}");
            Console.ForegroundColor = ConsoleColor.Blue;
            for (int i = 0; i < f.gameObjectReferences.Length; i++)
            {
                var e = f.gameObjectReferences[i];
                Console.WriteLine($"{i}: {e.name}; {e.objectType}; {e.coreType}; {e.index}; {e.unknown1}; {e.unknown2}; {e.unknown4}");
            }
            Console.ForegroundColor = ConsoleColor.Green;
            int favEmbed = -1;
            for (int i = 0; i < f.gameObjectEmbeds.Length; i++)
            {
                var e = f.gameObjectEmbeds[i];
                if (/*e.unknown5 == 11*/ e.dataLocation > 83789)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    favEmbed = i;
                }
                else
                    Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(i + ": " + e.type + " @ " + e.dataLocation + " -> " + e.dataLength + $"   2:{e.unknown2}; 3:{e.unknown3}; 4:{e.unknown4}; 5:{e.unknown5}; 6:{e.unknown6}; 7:{e.unknown7}; 8:{e.unknown8}; 9:{e.unknown9}; 10:{e.unknown10}; 11:{e.unknown11}; 12:{e.unknown12}; 13:{e.unknown13}; 14:{e.unknown14}");
                if (favEmbed != -1)
                    Console.WriteLine("");
            }
        }
    }
}
