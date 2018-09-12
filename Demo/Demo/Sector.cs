namespace SimpleFileSystem
{
    public class Sector
    {
        public int Id { get; set; }
        public byte[] Bytes { get; set; }
        public int Size { get; set; }

        public Sector(byte[] bytes, int id)
        {
            Bytes = bytes;
            Id = id;
        }
        
        
    }
}