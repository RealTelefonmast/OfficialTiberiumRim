using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TiberiumRim
{
    public interface IDraggable
    {
        public Vector2 Position { get; }
        public Rect? DragContext { get; }
        public Rect Rect { get; }
        public int Priority { get; }

        public void SetPosition(Vector2 newPos);
    }
}
