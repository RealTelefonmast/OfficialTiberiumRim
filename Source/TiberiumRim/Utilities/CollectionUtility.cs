using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiberiumRim.Utilities
{
    public static class CollectionUtility
    {
        public static void Populate<T>(this T[] array, IEnumerable<T> values)
        {
            int i = 0;
            foreach (var value in values)
            {
                array[i] = value;
                i++;
            }
        }
    }
}
