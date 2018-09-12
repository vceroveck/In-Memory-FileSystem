using System;
using System.Linq;
using System.Text;
using SimpleFileSystem;

namespace Demo
{
    class Program
    {
        static string CommandDescription(string command)
        {
            string commandDescription = "";
            switch (command)
            {
                case "touch":
                    commandDescription += "NAME: touch - create new, empty file" + Environment.NewLine;
                    commandDescription += "SYNOPSIS: touch FILENAME";
                    break;
                case "mkdir":
                    commandDescription += "NAME: mkdir - create new directory" + Environment.NewLine;
                    commandDescription += "SYNOPSIS: mkdir DIRECTORY" + Environment.NewLine;
                    break;
                case "rm":
                    commandDescription += "NAME: rm - remove files or directories" + Environment.NewLine;
                    commandDescription += "SYNOPSIS: rm [OPTION] FILE" + Environment.NewLine;
                    commandDescription += "OPTIONS: -dir -remove directory" + Environment.NewLine;
                    break;
                case "cp":
                    commandDescription += "NAME: cp - copy files or directories" + Environment.NewLine;
                    commandDescription += "SYNOPSIS: cp [OPTION] FILE" + Environment.NewLine;
                    commandDescription += "OPTIONS: -dir -copy directory" + Environment.NewLine;
                    break;
                case "cat":
                    commandDescription += "NAME: cat - print file on the standard output" + Environment.NewLine;
                    commandDescription += "SYNOPSIS: cat FILE" + Environment.NewLine;
                    break;
            }
            return commandDescription;
        }

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
                        if (fields[1].Contains("C:"))
                        {
                            fh = fs.Create(fields[1], bool.Parse(fields[2]), bool.Parse(fields[3]));
                        }
                        else
                        {
                            fh = fs.Create(fs.CurrentPath + fields[1], bool.Parse(fields[2]), bool.Parse(fields[3]));
                        }
                        break;
                    case "mkdir":
                        if (fields[1].Contains("C:"))
                        {
                            fh = fs.Create(fields[1], true, true);
                        }
                        else
                        {
                            fh = fs.Create(fs.CurrentPath + fields[1], true, true);
                        }
                        break;
                    case "rm":
                        if (fields[1] == "-dir")
                        {
                            if (!fs.DeleteDirectory(fields[2]))
                            {
                                Console.WriteLine("Diretory doesn't exist.");
                            }
                        }
                        else
                        {
                            if (fields[1].Contains("C:"))
                            {
                                fs.Delete(fields[1]);
                            }
                            else
                            {
                                fs.Delete(fs.CurrentPath + fields[1]);
                            }
                        }
                        break;
                    case "cat":
                        if (fields[1].Contains("C:"))
                        {
                            fh = fs.Open(fields[1]);

                        }
                        else
                        {
                            fh = fs.Open(fs.CurrentPath + fields[1]);
                        }
                        if (fh == null) Console.WriteLine("File doesn't exist.");
                        else
                        Console.WriteLine(Encoding.UTF8.GetString(fh?.ReadAll()));
                        break;
                    case "write":
                        if (fields[1].Contains("C:"))
                        {
                            fh = fs.Open(fields[1]);
                            fh?.Write(Encoding.UTF8.GetBytes(fields[2]));
                        }
                        else
                        {
                            fh = fs.Open(fs.CurrentPath + fields[1]);
                            fh?.Write(Encoding.UTF8.GetBytes(fields[2]));
                        }
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
                    case "man":
                        if (fields.Count() < 2)
                        {
                            Console.WriteLine("Which command do you want to see.");
                        }
                        else
                        {
                            Console.WriteLine(CommandDescription(fields[1]));
                        }
                        break;
                }
            }
        }
    }
}