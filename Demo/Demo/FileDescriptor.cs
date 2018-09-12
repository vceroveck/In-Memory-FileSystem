using System;
using System.Collections.Generic;

namespace SimpleFileSystem
{
    public class FileDescriptor
    {
        public string Path { get; set; }
        public bool Readable { get; set; }
        public bool Writeable { get; set; }
        
        public IList<Sector> Sectors { get; set; }

        public FileDescriptor(string path, bool isReadable, bool isWritable)
        {
            Path = path;
            Sectors = new List<Sector>();
            Readable = isReadable;
            Writeable = isWritable;
        }
    }
}
