using dspatch.DS;
using dspatch.IO;
using dspatch.Nitro;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace dspatch
{
    class Program
    {
        static byte[] dsHash = //Hash of the DS Download Station ROM
        {
            0xF1, 0x8B, 0x55, 0xF3, 0xE1, 0x25, 0x9C, 0x03, 0xE1, 0x0D, 0x0E, 0xCB,
            0x54, 0x96, 0x93, 0xB4, 0x29, 0x05, 0xCE, 0xB5
        };

        static void PrintUsage() //Won't be used
        {
            Console.WriteLine("Usage: dspatch -s download_station.nds -o result.nds [-i rom1.nds] [-I romfolder1]");
            Console.WriteLine("You can add multiple roms by using -I or -i multiple times");
        }

        static void Main(string[] args) //I will include credits
        {
            Console.WriteLine("== DS Download Station Patcher v1.0 ==");
            Console.WriteLine("Exploit by Gericom, shutterbug2000 and Apache Thunder\n");

            if (args.Length <= 1)
            {
                PrintUsage();
                return;
            }

            string dsPath = null; //Init variable for the path to the DS Download Station ROM
            string outPath = null; //Init variable for path to write the patched ROM to
            List<string> romPaths = new List<string>(); //Init list of ROMs to include in the patched ROM
            string filePath;
            //parse arguments
            int q = 0; //Unknown; Will look into later
            while (q < args.Length - 1)
            {
                string arg = args[q++];
                switch (arg)
                {
                    case "-s": //DS Download Station ROM
                        filePath = args[q++];
                        if (!File.Exists(filePath))
                        {
                            Console.WriteLine("Error: File (" + filePath + ") does not exist!");
                            return;
                        }
                        dsPath = filePath;
                        break;
                    case "-o": //Output ROM Path
                        outPath = args[q++];
                        break;
                    case "-i": //File to include in patched ROM
                        filePath = args[q++];
                        if (!File.Exists(filePath))
                        {
                            Console.WriteLine("Error: File (" + filePath + ") does not exist!");
                            return;
                        }
                        romPaths.Add(filePath); //Add the file to the list of ROMs
                        break;
                    case "-I": //Directory; may not add to GUI, still may if I have time.
                        string dirPath = args[q++];
                        if (!Directory.Exists(dirPath))
                        {
                            Console.WriteLine("Error: Directory (" + dirPath + ") does not exist!");
                            return;
                        }
                        romPaths.AddRange(Directory.GetFiles(dirPath, "*.nds"));
                        romPaths.AddRange(Directory.GetFiles(dirPath, "*.srl"));
                        break;
                    default:
                        Console.WriteLine("Error: Invalid argument (" + arg + ")\n");
                        PrintUsage();
                        return;
                }
            }

            if (dsPath == null)
            {
                Console.WriteLine("Error: Specify a download station rom!\n");
                PrintUsage();
                return;
            }
            if (outPath == null)
            {
                Console.WriteLine("Error: Specify a destination path!\n");
                PrintUsage();
                return;
            }
            if (romPaths.Count == 0)
            {
                Console.WriteLine("Error: Specify at least 1 rom!\n");
                PrintUsage();
                return;
            }

            byte[] dsdata = File.ReadAllBytes(dsPath); //Read the DS Download Station ROM into an array of bytes
            byte[] sha1 = SHA1.Create().ComputeHash(dsdata); //Determine the SHA1 hash of the ROM
            //Compare the hash 20 times just in case a mistake is made
            for (int i = 0; i < 20; i++)
            {
                if (sha1[i] != dsHash[i])
                {
                    Console.WriteLine("Error: Invalid download station rom!");
                    Console.WriteLine("The patcher is only compatible with:");
                    Console.WriteLine("xxxx - DS Download Station - Volume 1 (Kiosk WiFi Demo Cart) (U)(Independent).nds");
                    Console.WriteLine("SHA1: F18B55F3E1259C03E10D0ECB549693B42905CEB5");
                    return;
                }
            }
            DownloadStationPatcher p = new DownloadStationPatcher(new NDS(dsdata)); //Use the patcher to create the patched ROM
            foreach(var r in romPaths)
                p.AddRom(new NDS(File.ReadAllBytes(r))); //Add all of the ROMs in the list of ROMs
            byte[] finalResult = p.ProduceRom().Write(); //Write the bytes of the final, patched ROM
            File.Create(outPath).Close(); //Create and then close the file
            File.WriteAllBytes(outPath, finalResult); //Write the bytes of the patched ROM to the file for the patched ROM
            //We are done!
        }
    }
}
