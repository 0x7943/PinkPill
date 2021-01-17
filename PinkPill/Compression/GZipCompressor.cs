using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinkPill.Compression
{
    // 经过一系列测试 GZip对于短文字特别差
    public static class GZipCompressor
    {
        public static byte[] CompressBytes(byte[] bytes)
        {
            using var inputStream = new MemoryStream(bytes);
            using var outputStream = new MemoryStream();
            var gzipStream = new GZipStream(outputStream, CompressionLevel.Optimal);
            inputStream.CopyTo(gzipStream);
            gzipStream.Close();
            return outputStream.ToArray();
        }

        public static byte[] DecompressBytes(byte[] bytes)
        {
            using var sourceMs = new MemoryStream(bytes);
            var gzipStream = new GZipStream(sourceMs, CompressionMode.Decompress);
            using var resultMs = new MemoryStream();
            gzipStream.CopyTo(resultMs);
            gzipStream.Close();
            return resultMs.ToArray();
        }
    }
}
