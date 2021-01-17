using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinkPill.Encryption
{
    public interface IEncrypter
    {
        // 小朋友 动动脑筋 这里该写什么呢?
        Span<byte> Encrypt(Span<byte> sourceBytes, IEncryptionKey keyParameter);
        Span<byte> Decrypt(Span<byte> encryptedBytes, IEncryptionKey keyParameter);
    }

    public interface IEncryptionKey
    {
    }
}
