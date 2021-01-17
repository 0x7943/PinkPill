using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinkPill
{
    public ref struct PinkPillWriter
    {
        public Span<byte> Data;
        public byte[] ByteArray;
        int _position;
        
        public PinkPillWriter(int length)
        {
            ByteArray = new byte[length];
            Data = new Span<byte>(ByteArray);
            _position = 0x0;
        }

        public void Write(byte b)
        {
            Data[_position] = b;
            _position++;
        }

        public void Write(Span<byte> bytes)
        {
            bytes.CopyTo(Data[_position..]);
            _position += bytes.Length;
        }
        
        public void Write(ReadOnlySpan<byte> bytes)
        {
            bytes.CopyTo(Data[_position..]);
            _position += bytes.Length;
        }
    }
}
