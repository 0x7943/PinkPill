using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinkPill.Obfuscation
{
    public static class PostObfuscator
    {
        public static string Obfuscate(string source)
        {
            var length = source.Length;
            if (length < 5) return source;

            const double chance = 0.08;
            var sb = new StringBuilder(length + 20);
            
            foreach (var c in source)
            {
                AddSpace();
                sb.Append(c);
                AddSpace();
            }

            void AddSpace()
            {
                while (true)
                {
                    if (PinkPillRandom.NextDouble() < chance)
                    {
                        sb!.Append(' ');
                        continue;
                    }

                    break;
                }
            }

            return sb.ToString();
        }

        public static string Deobfuscate(string source)
        {
            return source.Replace(" ", "");
        }
    }
}
