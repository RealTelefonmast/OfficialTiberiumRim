using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TiberiumRim
{
    public struct WrappedTexture
    {
        public readonly string path;
        public readonly Texture texture;

        public WrappedTexture(string path, Texture texture)
        {
            this.path = path;
            this.texture = texture;
        }
    }
}
