using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinkPill.Obfuscation
{
    public interface IObfuscator<T>
    {
        T Obfuscate(ReadOnlySpan<byte> source);
        ReadOnlySpan<byte> Deobfuscate(T source);
        bool IsValid(T source);
    }

    public static class ObfuscatorExtensions
    {
        internal static bool ValidateUsingException<T>(this IObfuscator<T> obfuscator, T source)
        {
            try
            {
                obfuscator.Deobfuscate(source);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
