using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PinkPill
{
    public ref struct PinkPillWalker
    {
        public Span<byte> Bytes;
        public int Length => Bytes.Length;
        int _position;
        
        public PinkPillWalker(Span<byte> bytes)
        {
            Bytes = bytes;
            _position = 0x0;
        }

        public void SkipIf(byte b)
        {
            while (Peek() == b) MoveNext();
        }
        
        public void SkipUntil(byte b)
        {
            while (Peek() != b) MoveNext();
        }

        public void MoveNext()
        {
            CheckLength(0x1);
            _position++;
        }

        void CheckLength(int length)
        {
            if (_position > Length || length > Length - _position) throw new EndOfBytesException();
        }

        public ReadOnlySpan<byte> ReadBlock(int count)
        {
            CheckLength(count);
            
            var result = Bytes[_position..(_position+count)];
            _position += count;
            return result;
        }
        
        public ReadOnlySpan<byte> ReadToEnd()
        {
            var result = Bytes[_position..Length];
            _position = Length;
            return result;
        }
        
        public byte Read()
        {
            CheckLength(0x1);
            return Bytes[_position++];
        }

        public int ReadInt()
        {
            return BitConverter.ToInt32(ReadBlock(sizeof(int)));
        }

        public byte Peek()
        {
            CheckLength(0x1);
            return Bytes[_position];
        }

        public Span<byte> PeekBlock(int count)
        {
            CheckLength(count);
            return Bytes[_position..count];
        }
    }

    public static class Test
    {
    }
    [Serializable]
    public class EndOfBytesException : Exception
    {
        public EndOfBytesException()
        {
        }

        public EndOfBytesException(string message) : base(message)
        {
        }

        public EndOfBytesException(string message, Exception inner) : base(message, inner)
        {
        }

        protected EndOfBytesException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
