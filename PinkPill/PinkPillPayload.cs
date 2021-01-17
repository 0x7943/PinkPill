using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PinkPill
{
    public class PinkPillPayload
    {
        static readonly byte[] Header = BitConverter.GetBytes(0x7943_8964).Reverse().ToArray();
        static readonly byte CurrentVersion = 0x1;
        public byte[] Data { get; private set; }
        public byte EncryptMethod { get; private set; }
            
        // no public constructor
        PinkPillPayload() { }

        // Decode
        public static PinkPillPayload FromEncoded(Span<byte> encodedBytes)
        {
            var walker = new PinkPillWalker(encodedBytes);
            byte version;
            try
            {
                walker.SkipUntil(Header[0x0]); // 别倒垃圾
                var header = walker.ReadBlock(Header.Length);
                if (!header.SequenceEqual(Header)) throw new PPPDecodeException("Invalid header.");
                version = walker.Read();
            }
            catch (EndOfBytesException)
            {
                throw new PPPDecodeException("Invalid header");
            }

            var instance = new PinkPillPayload();
            switch (version)
            {
                case 0x1:
                    DecodeVersion1(ref instance, walker);
                    break;
                default:
                    throw new PPPDecodeException("Invalid version");
            }

            return instance;
        }

        static void DecodeVersion1(ref PinkPillPayload payload, PinkPillWalker walker)
        {
            payload.EncryptMethod = walker.Read();
            payload.Data = walker.ReadToEnd().ToArray();
        }

        public static PinkPillPayload FromBytes(Span<byte> bytes)
        {
            return new() { Data = bytes.ToArray() };
        }

        // 测试 10000000 次 GC 了 304 次
        // 有待优化
        public Span<byte> Encode()
        {
            // 给 base64 倒一点垃圾可以让整个字符串完全不同
            var garbageBytesCount = PinkPillRandom.NextInt(0x0, 0x6);
            var writer = new PinkPillWriter(garbageBytesCount + Header.Length + 1 /*version*/ + Data.Length);
            
            var garbageFilled = 0x0;
            while (garbageFilled < garbageBytesCount)
            {
                var toFill = (byte)PinkPillRandom.NextInt(0x0, byte.MaxValue);
                if (toFill == Header[0x0]) continue;
                writer.Write(toFill);
                garbageFilled++;
            }
            writer.Write(Header);
            writer.Write(CurrentVersion);
            writer.Write(EncryptMethod);
            writer.Write(Data);
            
            return writer.Data;
        }

    }

    [Serializable]
    public class PPPDecodeException : Exception
    {
        public PPPDecodeException(string message) : base(message)
        {
        }

        public PPPDecodeException(string message, Exception inner) : base(message, inner)
        {
        }

        protected PPPDecodeException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }

}
