using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonResources
{
    public static class HashHelper
    {
        private const ulong FnvPrime = 1099511628211;
        private const ulong FnvOffsetBasis = 14695981039346656037;

        public static long GetExtendedHash(this string value)
        {
            return Encoding.UTF32.GetBytes(value).GetExtendedHash();
        }

        public static long GetExtendedHash(this byte[] value)
        {
            ulong hash = FnvOffsetBasis;
            for (int i = 0; i < value.Length; ++i)
            {
                hash ^= value[i];
                hash *= FnvPrime;
            }

            return (long)hash;
        }
    }
}
