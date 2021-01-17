using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;

namespace PinkPill.Encryption
{
    public class EncryptionManager
    {
        static Dictionary<byte, IEncrypter> _encrypterRegistry = new ();
        static Dictionary<Type, IEncrypter> _encrypterTypesRegistry = new ();

        static EncryptionManager()
        {
            // 手动填写ID以保证向后兼容性
            RegisterEncrypter(new PasswordEncryption(), 0x0);
            
        }
        
        public static void RegisterEncrypter(IEncrypter encrypter, byte id)
        {
            var encrypterInRegistry = GetEncrypter(id);
            if (encrypterInRegistry != null)
                throw new ArgumentException($"Encrypter ID [{id}] already registered: {encrypterInRegistry.GetType().Name}");
            _encrypterRegistry[id] = encrypter;
            _encrypterTypesRegistry[encrypter.GetType()] = encrypter;
        }

        public static IEncrypter GetEncrypter(byte id) => _encrypterRegistry[id] ?? throw new ArgumentException($"Encrypter ID {id} not registered.");
        
    }
}
