using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ImageMagick;
using PinkPill.Obfuscation;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace PinkPill.ImageObfuscation
{
    public class ImageObfuscator : IObfuscator<MagickImage>
    {
        const uint Version = 0x0000_0001u;

        static readonly byte[] Header =
        {
                 0x79,      0x43,      0x89,      0x64, 
            (byte)'P', (byte)'i', (byte)'n', (byte)'k', 
            (byte)'P', (byte)'i', (byte)'l', (byte)'l'
        };
        /**
         * PinkPill Image Payload Format v1
         *
         * [Name]         [Bytes] [Content]
         * Header         12      0x7943_8964 + "PinkPill"
         * Version        4(uint) 0x0000_0001
         * Content-Length 4(int)  Bytes count for payload data
         * Content        *       The data this payload carries
         *
         */
        public MagickImage Obfuscate(ReadOnlySpan<byte> source)
        {
            var length = Header.Length + sizeof(int) + sizeof(int) + source.Length;
            var lengthInPixels = (int)Math.Ceiling(length / 3.0);
            var baseLength = Math.Ceiling(Math.Sqrt(lengthInPixels));

            // Pre calculated value
            // For density 72 and fontsize 16.2, the text "  [Pink Pill]  " (15 length) produces a image in 150*20
            var minWidth = 65;
            var ppidBaseHeight = 10.0;

            var width = (int)Math.Ceiling(Math.Max(minWidth, baseLength));
            var ppidSizeRatio = width / (double)minWidth;
            var ppidHeight = (int)Math.Ceiling(ppidSizeRatio * ppidBaseHeight);
            var dataPartHeight = (int)Math.Ceiling(lengthInPixels / (double)width);
            var height = ppidHeight + dataPartHeight;

            using var ppidImage = new MagickImage(new MagickColor("#2d2e33"), width, ppidHeight);
            
            ppidImage.Settings.Density = new Density(72);
            ppidImage.Settings.FontPointsize = 16.2 / 2.0 * ppidSizeRatio;
            ppidImage.Settings.Font = "FiraCode-Regular.ttf";
            ppidImage.Settings.TextAntiAlias = false;
            ppidImage.Settings.StrokeColor = new MagickColor(255, 255, 255);
            ppidImage.Settings.FillColor = new MagickColor(255, 127, 195);
            
            ppidImage.Annotate(" [Pink Pill] ", new MagickGeometry(0, (int)Math.Ceiling(ppidHeight / 2.0)), Gravity.Northwest);
            
            var ppidImageBytes = ppidImage.GetPixels().ToByteArray(PixelMapping.RGB);
            var writer = new PinkPillWriter(height * width * 3);
            writer.Write(ppidImageBytes);
            writer.Write(Header);
            writer.Write(BitConverter.GetBytes(Version));
            writer.Write(BitConverter.GetBytes(source.Length));
            writer.Write(source);
            //var loadPixelData = Image<Rgb24>.LoadPixelData(new Rgb24[2], 2,3);
            //loadPixelData.Frames.RootFrame.pix
            var outImage = new MagickImage(new MagickColor("#2d2e33"), width, height);
            outImage.ReadPixels(writer.ByteArray, new PixelReadSettings(width, height, StorageType.Char, PixelMapping.RGB));

            return outImage;
        }

        public ReadOnlySpan<byte> Deobfuscate(MagickImage source)
        {
            var imageBytes = source.GetPixels().ToByteArray(PixelMapping.RGB);
            var reader = new PinkPillWalker(imageBytes);
            reader.SkipUntil(Header[0]);
            var headerRead = reader.ReadBlock(Header.Length);
            if (!headerRead.SequenceEqual(Header)) throw new PPIDecodeException();
            var version = BitConverter.ToUInt32(reader.ReadBlock(sizeof(uint)));

            var length = BitConverter.ToInt32(reader.ReadBlock(sizeof(int)));
            return reader.ReadBlock(length);
        }

        public bool IsValid(MagickImage source)
        {
            return this.ValidateUsingException(source);
        }
    }

    [Serializable]
    public class PPIDecodeException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public PPIDecodeException()
        {
        }

        public PPIDecodeException(string message) : base(message)
        {
        }

        public PPIDecodeException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
