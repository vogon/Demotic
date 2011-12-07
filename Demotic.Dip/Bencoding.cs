using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Demotic.Dip
{
    public static partial class Bencoding
    {
        private static T[] JoinArrays<T>(IEnumerable<T[]> arrs)
        {
            T[] joined = new T[arrs.Sum(arr => arr.Length)];
            int start = 0;

            foreach (T[] arr in arrs)
            {
                Array.Copy(arr, 0, joined, start, arr.Length);
                start += arr.Length;
            }

            return joined;
        }        
    }
}
