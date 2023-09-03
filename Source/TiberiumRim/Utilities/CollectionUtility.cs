using System.Collections.Generic;

namespace TR.Utilities
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
