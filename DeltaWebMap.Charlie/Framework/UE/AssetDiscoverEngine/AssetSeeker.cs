using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace DeltaWebMap.Charlie.Framework.UE.AssetDiscoverEngine
{
    public class AssetSeeker
    {
        UEInstall install;
        string[] blacklist;
        Regex[] blacklistRegex;

        public AssetSeeker(UEInstall install, string[] blacklist)
        {
            this.install = install;
            this.blacklist = blacklist;
            this.blacklistRegex = new Regex[blacklist.Length];
            for (int i = 0; i < blacklist.Length; i++)
                blacklistRegex[i] = new Regex(blacklist[i]);
        }

        public Dictionary<string, DiscoveredFileType> SeekAssets(CharliePersist session)
        {                     
            //Discover all files that we will use by doing a basic, binary, search
            List<DiscoveredFile> files = new List<DiscoveredFile>();
            long bytesRead = 0;
            int itemsRead = 0;
            int regexSkipped = 0;
            bool isRunning = true;

            //Set up logging thread
            Thread log = new Thread(() =>
            {
                while(isRunning)
                {
                    string logMsg;
                    if(files.Count == 0)
                        logMsg = ($"Read {itemsRead} files, with {files.Count} of which being readable. {regexSkipped} regex skipped. {bytesRead / 1000 / 1000} MB read.                                                                      ");
                    else
                        logMsg = ($"Read {itemsRead} files, with {files.Count} of which being readable. {regexSkipped} regex skipped. {bytesRead / 1000 / 1000} MB read. Last: "+files[files.Count - 1].pathname.Substring(install.info.FullName.Length)+"              ");
                    while (logMsg.Length < Console.WindowWidth)
                        logMsg += " ";
                    Console.Write("\r" + logMsg);
                    Thread.Sleep(50);
                }
            });
            log.IsBackground = true;
            log.Start();

            //Discover
            DiscoverDirectory(install.info.FullName, files, ref bytesRead, ref itemsRead, ref regexSkipped);
            isRunning = false;

            //Create response
            var discovery = new Persist.DiscoveryFile();
            foreach (var f in files)
                discovery.files.Add(f.pathname, f.type);

            return discovery.files;
        }

        private void DiscoverDirectory(string path, List<DiscoveredFile> output, ref long bytesRead, ref int itemsRead, ref int regexSkipped)
        {
            //Read
            string[] files = Directory.GetFiles(path);
            string[] dirs = Directory.GetDirectories(path);

            //Loop files
            foreach(var f in files)
            {
                long thisSize;

                //Check if this is on the regex blacklist
                if(RegexCheckFile(f))
                {
                    //This is on the allowed name list, but we'll need to do the binary check
                    DiscoveredFileType type = CheckFile(f, out thisSize);

                    //Add
                    if ((int)type >= 0)
                    {
                        output.Add(new DiscoveredFile
                        {
                            type = type,
                            pathname = f
                        });
                    }
                } else
                {
                    //We can safely skip this.
                    thisSize = new FileInfo(f).Length;
                    regexSkipped++;
                }

                //Add info
                itemsRead++;
                bytesRead += thisSize;
            }

            //Loop subdirs
            foreach(var f in dirs)
            {
                DiscoverDirectory(f, output, ref bytesRead, ref itemsRead, ref regexSkipped);
            }
        }

        private bool RegexCheckFile(string pathname)
        {
            if (!pathname.EndsWith(".uasset"))
                return false;
            string matchName = pathname.Replace('\\', '/');
            foreach (var r in blacklistRegex)
            {
                if (r.IsMatch(matchName))
                {
                    return false;
                }
            }
            return true;
        }

        private DiscoveredFileType CheckFile(string pathname, out long size)
        {
            //We're simply going to read the name table and check if any of the name table entries match a specific string that is generally only found in dinos.

            //We require a little endian system. This shouldn't be a problem, but we'll ensure this is correct
            if (!BitConverter.IsLittleEndian)
                throw new Exception("Only little endian systems are supported.");

            //Run binary check
            using (FileStream fs = new FileStream(pathname, FileMode.Open, FileAccess.Read))
            {
                //Set size
                size = fs.Length;
                
                //Skip to content we care about
                fs.Position += 7 * 4;
                int temp = IOMemoryStream.StaticReadInt32(fs, true);
                if (temp > 128)
                    return DiscoveredFileType.Unreadable; //This is not a valid file.
                fs.Position += temp + 4;

                //Read length and position of the name table
                int ntLen = IOMemoryStream.StaticReadInt32(fs, true);
                int ntPos = IOMemoryStream.StaticReadInt32(fs, true);

                //Read next offset
                fs.Position += 4 * 4;
                int bitTablePos = IOMemoryStream.StaticReadInt32(fs, true);

                //Jump to the name table start position and begin reading.
                if (ntPos > fs.Length || ntPos <= 0 || ntLen > fs.Length || ntLen <= 0 || bitTablePos > fs.Length || bitTablePos <= 0 || ntPos > bitTablePos)
                    return DiscoveredFileType.Unreadable; //This is not a valid file.
                fs.Position = ntPos;

                //Read in from the name table to the binary id table. This is more data than we'll actually *use*, but it's better than querying the disk over and over
                byte[] buffer = new byte[bitTablePos - ntPos];
                fs.Read(buffer, 0, buffer.Length);

                //Read each string from the name table we loaded into the buffer
                int offset = 0;
                for(int i = 0; i<ntLen; i++)
                {
                    //Read length
                    int len = BitConverter.ToInt32(buffer, offset);
                    offset += 4;

                    //Check length
                    if (len > 256 || len < 1)
                        return DiscoveredFileType.Unreadable; //This is not a valid file.

                    //Read
                    string s = Encoding.UTF8.GetString(buffer, offset, len - 1);
                    offset += len;

                    //Compare
                    if (s == "ShooterCharacterMovement")
                        return DiscoveredFileType.Dino;
                    if (s == "DescriptiveNameBase" || s == "ItemIconMaterialParent")
                        return DiscoveredFileType.Item;
                    if (s == "StructureMesh")
                        return DiscoveredFileType.Structure;
                }

                //This is not a correct file.
                return DiscoveredFileType.None;
            }
        }
    }
}
