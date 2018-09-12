using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using SimpleFileSystem;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("1. Loading filesystem from file. 2. Create new filesystem.");

            var choice = Console.ReadLine();

            Console.WriteLine("Insert number of sectors and sector size: (number size)");

            var diskSizeArray = Console.ReadLine().Split(" ");

            int numberOfSectors = int.Parse(diskSizeArray[0]);
            int sectorSize = int.Parse(diskSizeArray[1]);

            FileSystem fs = new FileSystem(numberOfSectors, sectorSize);

            if (choice == "1")
            {
                Console.WriteLine("Insert file name: ");
                var fileName = Console.ReadLine();
                fs = FileSystemPersistor.Load(fileName, 1024, 1024);
                fs.CurrentPath = "C:\\";
                Console.WriteLine("File System loaded.");
            }
            
            FileHandle fh;
            
            while (true)
            {
                Console.Write(fs.CurrentPath + "> ");
                var input = Console.ReadLine();
                if (input == "exit")
                {
                    break;
                }

                var fields = input.Split(" ");
                switch (fields[0])
                {
                    case "touch":
                        if (fields[1].Contains("C:\\"))
                        {
                            fh = fs.Create(fields[1], bool.Parse(fields[2]), bool.Parse(fields[3]));
                        }
                        else
                        {
                            fh = fs.Create(fs.CurrentPath + fields[1], bool.Parse(fields[2]), bool.Parse(fields[3]));
                        }
                        break;
                    case "mkdir":
                        if (fields[1].Contains("C:\\"))
                        {
                            fh = fs.Create(fields[1], true, true);
                        }
                        else
                        {
                            fh = fs.Create(fs.CurrentPath + fields[1], true, true);
                        }
                        break;
                    case "rm":
                        fs.Delete(fs.CurrentPath + fields[1]);
                        break;
                    case "cat":
                        fh = fs.Open(fs.CurrentPath + fields[1]);
                        if (fh == null) Console.WriteLine("Greška!!!");
                        else
                        Console.WriteLine(Encoding.UTF8.GetString(fh?.ReadAll()));
                        break;
                    case "write":
                        fh = fs.Open(fs.CurrentPath + fields[1]);
                        fh?.Write(Encoding.UTF8.GetBytes(fields[2]));
                        break;
                    case "savefs":
                        FileSystemPersistor.Write(fields[1], fs);
                        Console.WriteLine("File System saved.");
                        break;
                    case "loadfs":
                        fs = FileSystemPersistor.Load(fields[1], 1024, 1024);
                        fs.CurrentPath = "C:\\";
                        Console.WriteLine("File System loaded.");
                        break;
                    case "size":
                        Console.WriteLine(fs.Size(fields[1]));
                        break;
                    case "cp":
                        if (fields[1] == "-dir")
                        {
                            if(!fs.CopyDirectory(fields[2], fields[3]))
                            {
                                Console.WriteLine("No such file or directory.");
                            }
                        }
                        else
                        {
                            if (!fs.Copy(fields[1], fields[2]))
                            {
                                Console.WriteLine("No such file or directory.");
                            }
                        }
                        break;
                    case "mv":
                        if (fields[1] == "-dir")
                        {
                            if (!fs.RenameDirectory(fields[2], fields[3]))
                            {
                                Console.WriteLine("No such directory.");
                            }
                        }
                        else
                        {
                            if (!fs.Rename(fields[1], fields[2]))
                            {
                                Console.WriteLine("No such file or directory.");
                            }
                        }
                        
                        break;
                    case "ls":
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine();
                        foreach (var directory in fs.ListDirectories(fs.CurrentPath))
                        {
                            Console.WriteLine(directory);
                        }
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write(fs.ListFiles(fs.CurrentPath));
                        break;
                    case "cd":
                        switch (fields[1])
                        {
                            case "..":
                                var directories = fs.CurrentPath.Split('\\');
                                fs.CurrentPath = "";
                                for (int i = 0; i < directories.Count() - 2; i++)
                                {
                                    fs.CurrentPath += directories[i] + "\\";
                                }
                                break;
                            default:
                                foreach (var directory in fs.ListDirectories(fs.CurrentPath))
                                {
                                    if (directory.ToString() == fields[1])
                                    {
                                        fs.CurrentPath += directory.ToString();
                                        break;
                                    }
                                }
                                break;
                        }
                        break;
                }
            }
        }
    }
}