using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SimpleFileSystem
{
    public class FileHandle
    {
        private int _currentSector;
        private int _currentByte;
        private readonly FileDescriptor _fileDescriptor;
        private readonly FileSystem _fileSystem;

        public FileHandle(FileDescriptor fileDescriptor, FileSystem fileSystem)
        {
            _fileDescriptor = fileDescriptor;
            _fileSystem = fileSystem;
        }

        public bool HasNext()
        {
            return _currentSector != _fileDescriptor.Sectors.Count;
        }

        public byte Read()
        {
            if (!_fileDescriptor.Readable)
            {
                throw new Exception("File is not readable.");
            }

            if (!HasNext())
            {
                throw new Exception("No more elements.");
            }
            
            byte data = _fileDescriptor.Sectors[_currentSector].Bytes[_currentByte++];

            if (_currentByte >= _fileDescriptor.Sectors[_currentSector].Size)
            {
                _currentSector++;
            }

            return data;
        }

        public void Write(byte data)
        {
            if (!_fileDescriptor.Writeable)
            {
                throw new Exception("File is not writeable.");
            }

            Sector lastSector = _fileDescriptor.Sectors[_fileDescriptor.Sectors.Count - 1];

            if (lastSector.Size == lastSector.Bytes.Length)
            {
                _fileDescriptor.Sectors.Add(_fileSystem.GetFreeSector());
            }

            lastSector = _fileDescriptor.Sectors[_fileDescriptor.Sectors.Count - 1];
            lastSector.Bytes[lastSector.Size++] = data;
        }

        public void Write(byte[] data)
        {
            foreach (var b in data)
            {
                Write(b);
            }
        }

        /// <summary>
        /// Reads remaining bytes.
        /// </summary>
        /// <returns></returns>
        public byte[] ReadAll()
        {
            IList<byte> bytes = new List<byte>();
            
            while (HasNext())
            {
                bytes.Add(Read());
            }

            return bytes.ToArray();
        }
    }
}