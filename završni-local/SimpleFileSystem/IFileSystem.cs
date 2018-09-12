namespace SimpleFileSystem
{
    public interface IFileSystem
    {
        FileHandle Open(string path);
    }
}