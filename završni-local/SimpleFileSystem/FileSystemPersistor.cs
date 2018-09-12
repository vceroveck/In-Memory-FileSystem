using System.IO;

namespace SimpleFileSystem
{
    public class FileSystemPersistor
    {
        public static FileSystem Load(string name, int numberOfSectors, int sectorSize)
        {
            byte[] bytes = File.ReadAllBytes(name);

            FileSystem fileSystem = FileSystem.Generate(numberOfSectors, sectorSize);

            fileSystem.Create(bytes, numberOfSectors, sectorSize);

            return fileSystem;
        }

        public static void Write(string name, FileSystem fileSystem)
        {
            byte[] bytes = fileSystem.GetCurrentState();

            File.WriteAllBytes(name, bytes);
        }
    }
}