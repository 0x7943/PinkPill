using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PinkPill.Obfuscation
{
    public class Base64Obfuscator : IObfuscator<string>
    {
        public string Obfuscate(ReadOnlySpan<byte> source) => Convert.ToBase64String(source);

        public ReadOnlySpan<byte> Deobfuscate(string source) => Convert.FromBase64String(source);

        public bool IsValid(string source) => Regex.IsMatch(source, "^@(?=(.{4})*$)[A-Za-z0-9+/]*={0,2}$");
    }
}
