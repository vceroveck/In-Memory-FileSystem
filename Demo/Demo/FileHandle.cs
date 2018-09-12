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
        public int ReadingFilePointer { get; set; }
        public int WritingFilePointer { get; set; }


        public FileHandle()
        {

        }
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
                _currentByte = 0;
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

            _currentSector = 0;
            _currentByte = 0;
            
            while (HasNext())
            {
                bytes.Add(Read());
            }

            return bytes.ToArray();
        }

        public byte[] ReadNumberOfBytes(int numberOfBytes)
        {
            IList<byte> bytes = new List<byte>();

            _currentSector = ReadingFilePointer / _fileSystem._sectorSize;
            _currentByte = ReadingFilePointer % _fileSystem._sectorSize;

            while (HasNext() && numberOfBytes-->0)
            {
                bytes.Add(Read());
                ReadingFilePointer++;
            }

            return bytes.ToArray();
        }
        
        public void WriteByte(byte data)
        {
            if (!_fileDescriptor.Writeable)
            {
                throw new Exception("File is not writeable.");
            }

            int tempCurrentSector = WritingFilePointer / _fileSystem._sectorSize;
            int tempCurrentByte = WritingFilePointer % _fileSystem._sectorSize;


            Sector lastSector = _fileDescriptor.Sectors[_fileDescriptor.Sectors.Count - 1];

            if (_fileDescriptor.Sectors[tempCurrentSector].Id == lastSector.Id)
            {
                if(tempCurrentByte == lastSector.Bytes.Length - 1)
                {
                    WritingFilePointer++;
                    Write(data);
                }
                else
                {
                    _fileDescriptor.Sectors[tempCurrentSector].Bytes[tempCurrentByte] = data;

                    WritingFilePointer++;

                    lastSector.Size++;
                }
            }
            else
            {

                _fileDescriptor.Sectors[tempCurrentSector].Bytes[tempCurrentByte] = data;

                WritingFilePointer++;
            }
        }
        public void WriteFromFilePointer(byte[] data)
        {
            foreach (var item in data)
            {
                WriteByte(item);
            }
        }

        public bool SetReadingFilePointer(int filePointer)
        {
            if (filePointer < ((_fileDescriptor.Sectors.Count) * (_fileSystem._sectorSize-1)))
            {
                ReadingFilePointer = filePointer;
                return true;
            }
            return false;
        }

        public bool SetWritingFilePointer(int filePointer)
        {
            if (filePointer < ((_fileDescriptor.Sectors.Count) * (_fileSystem._sectorSize - 1)))
            {
                WritingFilePointer = filePointer;
                return true;
            }
            return false;
        }

        public int TellWritingPosition()
        {
            return WritingFilePointer;
        }

        public int TellReadingPosition()
        {
            return ReadingFilePointer;
        }
    }
}