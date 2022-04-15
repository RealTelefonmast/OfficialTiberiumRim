﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TiberiumRim
{
    public struct WrappedTexture
    {
        private string path;
        private Texture texture;

        public string Path => path;
        public Texture Texture => texture;

        public WrappedTexture(string path, Texture texture)
        {
            this.path = path;
            this.texture = texture;
        }

        public void Clear()
        {
            path = null;
            texture = null;
        }
    }
}
