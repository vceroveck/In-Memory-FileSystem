using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SimpleFileSystem
{
    public class FileSystem : IFileSystem
    {
        private IDictionary<int, Sector> Sectors { get; set; }
        private IDictionary<string, FileDescriptor> Files { get; set; }
        public int _numberOfSectors { get; set; }
        public int _sectorSize { get; set; }

        public string CurrentPath { get; set; }

        public FileSystem(int numberOfSectors, int sectorSize)
        {
            _numberOfSectors = numberOfSectors;
            _sectorSize = sectorSize;
            CurrentPath = "C:\\";
            Sectors = new Dictionary<int, Sector>();
            Files = new Dictionary<string, FileDescriptor>();
            Create("C:\\", true, true);
        }

        public static FileSystem Generate(int numberOfSectors, int sectorSize)
        {
            return new FileSystem(numberOfSectors, sectorSize);
        }

        public FileSystem Create(byte[] bytes, int numberOfSectors, int sectorSize)
        {
            //prva dva sektora rezervirana za opisnike
            int i = 0;
            while (true)
            {
                string path = Encoding.UTF8.GetString(bytes.Skip(i).Take(100).ToArray());

                int numberOfCharactersInPath = Regex.Matches(path, @"[a-zA-Z]").Count;

                if (string.IsNullOrWhiteSpace(path) || numberOfCharactersInPath == 0 || i >= 2 * sectorSize)
                {
                    break;
                }

                path = path.TrimEnd();
                i += 100;

                var descriptor = new FileDescriptor(path, true, true);

                descriptor.Readable = (bytes[i] & 1) == 1;
                descriptor.Writeable = (bytes[i++] & 2) == 2;

                int fileSize = BitConverter.ToInt32(bytes, i);
                int numberOfFileSectors = fileSize / sectorSize + 1;
                i += 4;

                for (int j = 0; j < numberOfFileSectors; j++)
                {
                    int sectorNumber = BitConverter.ToInt32(bytes, i + j * 4);

                    Sectors[sectorNumber] =
                        new Sector(bytes.Skip(sectorNumber * sectorSize).Take(sectorSize).ToArray(), sectorNumber);

                    this.Sectors[sectorNumber].Size = _sectorSize;

                    descriptor.Sectors.Add(this.Sectors[sectorNumber]);
                }

                descriptor.Sectors.Last().Size = fileSize % sectorSize;
                AddFile(path, descriptor);

                i += numberOfFileSectors * 4;
            }

            return this;
        }


        public void AddFile(string path, FileDescriptor descriptor)
        {
            Files[path] = descriptor;
        }

        public static FileSystem CreateEmpty(int numberOfSectors, int sectorSize)
        {
            return new FileSystem(numberOfSectors, sectorSize);
        }

        public FileHandle Open(string path)
        {
            var keys = Files.Keys.ToArray();
            if (!Files.ContainsKey(path))
            {
                return null;
            }

            return new FileHandle(Files[path], this);
        }

        public FileHandle Create(string path, bool isReadable, bool isWriteable)
        {
            if (Files.ContainsKey(path))
            {
                return null;
            }

            FileDescriptor file = new FileDescriptor(path, isReadable, isWriteable);
            file.Sectors.Add(GetFreeSector());

            Files[path] = file;

            //provjera!
            return new FileHandle(Files[path], this);
        }

        public byte[] GetCurrentState()
        {
            List<byte> bytes = new List<byte>();

            foreach (var file in Files)
            {
                bytes.AddRange(Encoding.UTF8.GetBytes(file.Key.PadRight(100)));
                bytes.Add((byte)((file.Value.Readable ? 1 : 0) + (file.Value.Writeable ? 2 : 0)));
                bytes.AddRange(BitConverter.GetBytes((int)((file.Value.Sectors.Count - 1) * _sectorSize + file.Value.Sectors.Last().Size)));
                foreach (var sector in file.Value.Sectors)
                {
                    bytes.AddRange(BitConverter.GetBytes((int)sector.Id));
                }
            }

            bytes.AddRange(new byte[2 * _sectorSize - bytes.Count]);

            for (int i = 2; i < _numberOfSectors; i++)
            {
                if (Sectors.ContainsKey(i))
                {
                    bytes.AddRange(Sectors[i].Bytes);
                }
                else
                {
                    bytes.AddRange(new byte[_sectorSize]);
                }
            }

            return bytes.ToArray();
        }

        public Sector GetFreeSector()
        {
            for (int i = 2; i < _numberOfSectors; i++)
            {
                if (!Sectors.ContainsKey(i))
                {
                    Sectors[i] = new Sector(new byte[_sectorSize], i);
                    return Sectors[i];
                }
            }

            throw new OutOfMemoryException();
        }

        public bool Copy(string source, string destination)
        {
            var sourceFile = Open(source);
            if (sourceFile == null)
            {
                return false;
            }

            var destinationFile = Create(destination, true, true);
            while (sourceFile.HasNext())
            {
                destinationFile.Write(sourceFile.Read());
            }

            return true;
        }

        public bool CopyDirectory(string oldPath, string newPath)
        {
            int counter = 0;

            IDictionary<string, FileDescriptor> itemsToChange = new Dictionary<string, FileDescriptor>();

            foreach (var fileWithOldPath in Files)
            {
                if (Files[fileWithOldPath.Key].Path.Contains(oldPath))
                {
                    itemsToChange.Add(fileWithOldPath.Key, Files[fileWithOldPath.Key]);
                }
            }
            //cp -dir C:\Vinko\ C:\Zvonko\
            foreach (var item in itemsToChange)
            {
                counter++;
                string path = item.Key.Replace(oldPath, newPath);
                var file = Open(item.Key);
                var destinationFile = Create(path, Files[item.Key].Readable, Files[item.Key].Writeable);
                Console.WriteLine(path);
                while (file != null && file.HasNext() && path[path.Length - 1] != '\\')
                {
                    destinationFile.Write(file.Read());
                }
            }

            if (counter == 0) return false;

            return true;
        }

        public bool Delete(string target)
        {
            if (!Files.ContainsKey(target))
            {
                return false;
            }

            var file = Files[target];
            foreach (var sector in file.Sectors)
            {
                Sectors.Remove(sector.Id);
            }

            Files.Remove(target);

            return true;
        }

        public bool DeleteDirectory(string target)
        {
            if (!Files.ContainsKey(target))
            {
                return false;
            }

            IDictionary<string, FileDescriptor> filesToRemove = new Dictionary<string, FileDescriptor>();

            foreach (var file in Files)
            {
                if ((Files[file.Key].Path.Substring(0, Math.Min(Files[file.Key].Path.Length, target.Length))) == target)
                {
                    foreach (var sector in Files[file.Key].Sectors)
                    {
                        Sectors.Remove(sector.Id);
                    }
                    filesToRemove.Add(file.Key, file.Value);
                }
            }

            foreach (var file in filesToRemove)
            {
                Files.Remove(file.Key);
            }

            return true;
        }

        public bool Rename(string oldPath, string newPath)
        {
            if (!Files.ContainsKey(oldPath) || Files.ContainsKey(newPath))
            {
                return false;
            }

            var file = Files[oldPath];
            Files.Remove(oldPath);
            Files[newPath] = file;
            file.Path = newPath;

            return true;
        }

        public bool RenameDirectory(string oldPath, string newPath)
        {
            int counter = 0;

            IDictionary<string, FileDescriptor> itemsToChange = new Dictionary<string, FileDescriptor>();

            foreach (var fileWithOldPath in Files)
            {
                if (fileWithOldPath.Key.Contains(oldPath))
                {
                    itemsToChange.Add(fileWithOldPath.Key, Files[fileWithOldPath.Key]);
                }
            }
            foreach (var item in itemsToChange)
            {
                counter++;
                string path = item.Key.Replace(oldPath, newPath);
                var file = itemsToChange[item.Key];
                Files.Remove(item.Key);
                Files[path] = file;
                file.Path = path;
            }

            if (counter == 0) return false;

            return true;
        }

        public int Size(string path)
        {
            if (!Files.ContainsKey(path))
            {
                return -1;
            }

            return (Files[path].Sectors.Count - 1) * _sectorSize + Files[path].Sectors.Last().Size;
        }

        public string ListFiles(string path)
        {
            string filesNames = "";

            foreach (var filePath in Files)
            {
                string temp = filePath.Key;
                int lastBackSlashPosition = temp.LastIndexOf('\\');
                temp = temp.Substring(0, lastBackSlashPosition + 1);

                if (temp == path)
                {
                    filesNames += filePath.Key.Replace(temp, "") + Environment.NewLine;
                }
            }
            return filesNames;
        }


        public IList ListDirectories(string path)
        {

            IList directories = new List<string>();

            var files = Files.Keys.ToArray();

            foreach (var file in files)
            {
                string directory = "";
                if (file.Contains(path))
                {
                    for (int i = path.Count(); i < file.Count(); i++)
                    {
                        directory += file[i];
                        if (file[i] == '\\')
                        {
                            break;
                        }
                    }
                }
                if (directory.Contains('\\'))
                {
                    if (!directories.Contains(directory))
                    {
                        directories.Add(directory);
                    }
                }
            }

            return directories;
        }
    }
}