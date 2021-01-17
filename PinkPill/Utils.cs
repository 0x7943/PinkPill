using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PinkPill
{
    static class PinkPillRandom
    {
        static readonly ThreadLocal<Random> Rng = new (() => new Random());
        public static int NextInt(int min, int max)
        {
            return Rng.Value!.Next(min, max);
        }

        public static double NextDouble()
        {
            return Rng.Value!.NextDouble();
        }
    }
}
