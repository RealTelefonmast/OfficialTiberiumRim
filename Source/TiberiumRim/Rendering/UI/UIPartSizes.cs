using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiberiumRim
{
    public struct UIPartSizes
    {
        //public Dictionary<string, float> sortedSizes;
        public float[] sortedSizes;
        public float totalSize;

        public float this[int ind] => sortedSizes[ind];

        public UIPartSizes(int capacity)
        {
            
            sortedSizes = new float[capacity];
            totalSize = 0;
        }

        public void Register(int ind, float size)
        {
            //sortedSizes ??= new Dictionary<string, float>();
            sortedSizes[ind] = size;
            totalSize += size;
        }
    }
}
