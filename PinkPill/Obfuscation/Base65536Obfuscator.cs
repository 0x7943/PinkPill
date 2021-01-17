/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CyberDot.Encoding.Base65536;

namespace PinkPill.Obfuscation
{
    public class Base65536Obfuscator : IObfuscator<string>
    {
        public string Obfuscate(Span<byte> source)
        {
            return Base65536.Encode(source.ToArray());
        }

        public Span<byte> Deobfuscate(string source)
        {
            return Base65536.Decode(source);
        }
        
        public bool IsValid(string source)
        {
            return this.ValidateUsingException(source);
        }
    }
}
*/