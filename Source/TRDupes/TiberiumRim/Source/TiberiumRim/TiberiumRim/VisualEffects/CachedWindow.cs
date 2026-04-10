using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TiberiumRim
{
    public abstract class CachedWindow
    {

        //Strings should always be cached and recalled
        public string[] stringCache;

        protected CachedWindow()
        {
            Setup();
        }

        private void Setup()
        {
            CacheStrings();

        }

        public virtual void CacheStrings()
        { }

        public virtual void DrawParts(Rect innerRect)
        {

        }
    }
}
