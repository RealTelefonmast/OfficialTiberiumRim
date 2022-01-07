using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TiberiumRim
{
    public interface IResizable
    {
        public Vector2 Size { get; }

        public void SetSize(Vector2 newSize);
    }
}
