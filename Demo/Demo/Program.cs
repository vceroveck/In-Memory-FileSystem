using System;
using System.Linq;
using System.Text;
using SimpleFileSystem;
using System.Text.RegularExpressions;
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
                    commandDescription += "SYNOPSIS: touch FILE_PATH";
                    break;
                case "mkdir":
                    commandDescription += "NAME: mkdir - create new directory" + Environment.NewLine;
                    commandDescription += "SYNOPSIS: mkdir DIRECTORY_PATH" + Environment.NewLine;
                    break;
                case "rm":
                    commandDescription += "NAME: rm - remove files or directories" + Environment.NewLine;
                    commandDescription += "SYNOPSIS: rm [OPTION] FILE_PATH" + Environment.NewLine;
                    commandDescription += "OPTIONS: -dir -remove directory" + Environment.NewLine;
                    break;
                case "cp":
                    commandDescription += "NAME: cp - copy files or directories" + Environment.NewLine;
                    commandDescription += "SYNOPSIS: cp [OPTION] FILE_PATH" + Environment.NewLine;
                    commandDescription += "OPTIONS: -dir -copy directory" + Environment.NewLine;
                    break;
                case "cat":
                    commandDescription += "NAME: cat - print file on the standard output" + Environment.NewLine;
                    commandDescription += "SYNOPSIS: cat FILE_PATH" + Environment.NewLine;
                    break;
                case "openfile":
                    commandDescription += "NAME: openfile - open file" + Environment.NewLine;
                    commandDescription += "SYNOPSIS: openfile FILE_PATH";
                    break;
                case "wnbytes":
                    commandDescription += "NAME: wnbytes - write number of bytes in opened file" + Environment.NewLine;
                    commandDescription += "SYNOPSIS: wnbytes [SOME TEXT]";
                    break;
                case "rnbytes":
                    commandDescription += "NAME: rnbytes - read number of bytes from opened file" + Environment.NewLine;
                    commandDescription += "SYNOPSIS: rnbytes NUMBER";
                    break;
                case "swfp":
                    commandDescription += "NAME: swfp - set writing file pointer" + Environment.NewLine;
                    commandDescription += "SYNOPSIS: swfp NUMBER";
                    break;
                case "srfp":
                    commandDescription += "NAME: srfp - set reading file pointer" + Environment.NewLine;
                    commandDescription += "SYNOPSIS: srfp NUMBER";
                    break;
                case "cfs":
                    commandDescription += "NAME: cfs - create file system" + Environment.NewLine;
                    commandDescription += "SYNOPSIS: cfs NUMBER_OF_SECTORS SECTOR_SIZE";
                    break;
                case "dfs":
                    commandDescription += "NAME: dfs - delete file system" + Environment.NewLine;
                    commandDescription += "SYNOPSIS: dfs";
                    break;
                case "tellg":
                    commandDescription += "NAME: tellg - get position in input sequence" + Environment.NewLine;
                    commandDescription += "SYNOPSIS: tellg";
                    break;
                case "tellp":
                    commandDescription += "NAME: tellp - get position in output sequence" + Environment.NewLine;
                    commandDescription += "SYNOPSIS: tellp";
                    break;
            }
            return commandDescription;
        }

        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Enter 1 for loading filesystem from file. Enter 2 to create new filesystem.");

                var choice = Console.ReadLine();

                Console.WriteLine("Insert number of sectors and sector size: (<number> <size>)");

                var diskSizeArray = Console.ReadLine().Split(' ');

                FileHandle OpenedFile = null;

                int numberOfSectors = int.Parse(diskSizeArray[0]);
                int sectorSize = int.Parse(diskSizeArray[1]);

                FileSystem fs = new FileSystem(numberOfSectors, sectorSize);

                if (choice == "1")
                {
                    Console.WriteLine("Insert file name: ");
                    var fileName = Console.ReadLine();
                    fs = FileSystemPersistor.Load(fileName, numberOfSectors, sectorSize);
                    fs.CurrentPath = "C:\\";
                    Console.WriteLine("File System loaded.");
                }

                FileHandle fh;

                while (true && fs != null)
                {
                    Console.Write(fs.CurrentPath + "> ");
                    var input = Console.ReadLine();
                    if (input == "exit")
                    {
                        break;
                    }

                    var fields = input.Split(' ');
                    switch (fields[0])
                    {
                        case "cfs":
                            try
                            {
                                fs = new FileSystem(int.Parse(fields[1]), int.Parse(fields[2]));
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("cfs: missing operands");
                            }
                            break;
                        case "dfs":
                            fs = null;
                            break;
                        case "touch":
                            if (fields[1][fields[1].Length-1]=='\\')
                            {
                                Console.WriteLine("touch: wrong operand");
                                break;
                            }
                            try
                            {
                                if (fields[1].Contains("C:"))
                                {
                                    fh = fs.Create(fields[1], bool.Parse(fields[2]), bool.Parse(fields[3]));
                                }
                                else
                                {
                                    fh = fs.Create(fs.CurrentPath + fields[1], bool.Parse(fields[2]), bool.Parse(fields[3]));
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("touch: missing operands or wrong operands");
                            }
                            break;
                        case "mkdir":
                            try
                            {
                                if (fields[1].Contains("C:"))
                                {
                                    fs.Create(fields[1], true, true);
                                }
                                else
                                {
                                    fs.Create(fs.CurrentPath + fields[1], true, true);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("mkdir: missing operands or wrong operands");
                            }
                            break;
                        case "rm":
                            try
                            {
                                if (fields[1] == "-dir")
                                {
                                    if (fields[1] == "C:\\")
                                    {
                                        Console.WriteLine("rm: cannot remove root directory");
                                        break;
                                    }
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
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("rm: missing operands or wrong operands");
                            }
                            break;
                        case "cat":
                            try
                            {
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
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("cat: missing operands or wrong operands");
                            }
                            break;
                        case "write":
                            if (fields.Count() < 2)
                            {
                                Console.WriteLine("write: missing operands or wrong operands");
                                break;
                            }
                            Console.SetCursorPosition(0, Console.CursorTop + 1);
                            var textForFile = "";
                            while (true)
                            {
                                ConsoleKeyInfo ki = Console.ReadKey(true);
                                if ((ki.Key == ConsoleKey.Q) && (ki.Modifiers == ConsoleModifiers.Control))
                                {
                                    break;
                                }
                                if (ki.Key == ConsoleKey.Enter)
                                {
                                    Console.SetCursorPosition(0, Console.CursorTop + 1);
                                    textForFile += Environment.NewLine;
                                }
                                else if (ki.Key == ConsoleKey.LeftArrow)
                                {
                                    Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                                    break;
                                }
                                else if (ki.Key == ConsoleKey.UpArrow)
                                {
                                    Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop - 1);
                                    break;
                                }
                                else if (ki.Key == ConsoleKey.DownArrow)
                                {
                                    Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop + 1);
                                    break;
                                }
                                else if (ki.Key == ConsoleKey.RightArrow)
                                {
                                    Console.SetCursorPosition(Console.CursorLeft + 1, Console.CursorTop);
                                    break;
                                }
                                else
                                {
                                    Console.Write(ki.KeyChar);
                                    textForFile += ki.KeyChar;
                                }
                            }
                            try
                            {
                                if (fields[1].Contains("C:"))
                                {
                                    fh = fs.Open(fields[1]);
                                    fh?.Write(Encoding.UTF8.GetBytes(textForFile));
                                }
                                else
                                {
                                    fh = fs.Open(fs.CurrentPath + fields[1]);
                                    fh?.Write(Encoding.UTF8.GetBytes(textForFile));
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("write: missing operands or wrong operands");
                            }
                            break;
                        case "savefs":
                            if (fields.Count() < 2)
                            {
                                Console.WriteLine("savefs: missing operand");
                                break;
                            }
                            FileSystemPersistor.Write(fields[1], fs);
                            Console.WriteLine("File System saved.");
                            break;
                        case "loadfs":
                            try
                            {
                                fs = FileSystemPersistor.Load(fields[1], numberOfSectors, sectorSize);
                                fs.CurrentPath = "C:\\";
                                Console.WriteLine("File System loaded.");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("loadfs: missing operands or wrong operands");
                            }
                            break;
                        case "size":
                            Console.WriteLine(fs.Size(fields[1]));
                            break;
                        case "cp":
                            try
                            {
                                if (fields[1] == "-dir")
                                {
                                    if (!fs.CopyDirectory(fields[2], fields[3]))
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
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("cp: missing operands or wrong operands");
                            }
                            break;
                        case "mv":
                            try
                            {
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
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("mv: missing operands or wrong operands");
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
                        case "openfile":
                            if (fields.Count() < 2)
                            {
                                Console.WriteLine("openfile: missing operand");
                                break;
                            }
                            try
                            {
                                OpenedFile = fs.Open(fields[1]);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("openfile: no such file");
                            }
                            break;
                        case "closefile":
                            OpenedFile = null;
                            break;
                        case "srfp":
                            if (OpenedFile == null)
                            {
                                Console.WriteLine("There is no file opened!");
                                break;
                            }
                            try
                            {
                                fh = OpenedFile;
                                if (!fh.SetReadingFilePointer(int.Parse(fields[1])))
                                {
                                    Console.WriteLine("srfp: unknown error");
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("srfp: missing operand or wrong operand");
                            }
                            break;
                        case "swfp":
                            if (OpenedFile == null)
                            {
                                Console.WriteLine("There is no file opened!");
                                break;
                            }
                            try
                            {
                                fh = OpenedFile;
                                if (!fh.SetWritingFilePointer(int.Parse(fields[1])))
                                {
                                    Console.WriteLine("swfp: unknown error");
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("swfp: missing operand or wrong operand");
                            }
                            break;
                        case "rnbytes":
                            if (OpenedFile == null)
                            {
                                Console.WriteLine("There is no file opened!");
                                break;
                            }
                            try
                            {
                                fh = OpenedFile;
                                Console.WriteLine(Encoding.UTF8.GetString(fh.ReadNumberOfBytes(int.Parse(fields[1]))));
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("rnbytes: missing operand or wrong operand");
                            }
                            break;
                        case "wnbytes":
                            if (OpenedFile == null)
                            {
                                Console.WriteLine("There is no file opened!");
                                break;
                            }
                            try
                            {
                                fh = OpenedFile;
                                string text = "";
                                for (int i = 1; i < fields.Length; i++)
                                {
                                    text += fields[i];
                                }
                                fh.WriteFromFilePointer(Encoding.UTF8.GetBytes(text));
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("wnbytes: missing operand or wrong operand");
                            }
                            break;
                        case "tellp":
                            if (OpenedFile == null)
                            {
                                Console.WriteLine("There is no file opened!");
                                break;
                            }
                            fh = OpenedFile;
                            Console.WriteLine(fh.TellWritingPosition().ToString());
                            break;
                        case "tellg":
                            if (OpenedFile == null)
                            {
                                Console.WriteLine("There is no file opened!");
                                break;
                            }
                            fh = OpenedFile;
                            Console.WriteLine(fh.TellReadingPosition().ToString());
                            break;
                        case "help":
                            if (fields.Count() < 2)
                            {
                                Console.WriteLine("Which of following commands do you want to see?");
                                Console.WriteLine(" -touch");
                                Console.WriteLine(" -mkdir");
                                Console.WriteLine(" -rm");
                                Console.WriteLine(" -cp");
                                Console.WriteLine(" -cat");
                                Console.WriteLine(" -openfile");
                                Console.WriteLine(" -wnbytes");
                                Console.WriteLine(" -rnbytes");
                                Console.WriteLine(" -swfp");
                                Console.WriteLine(" -srfp");
                                Console.WriteLine(" -cfs");
                                Console.WriteLine(" -dfs");
                                Console.WriteLine(" -tellp");
                                Console.WriteLine(" -tellg");
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
}