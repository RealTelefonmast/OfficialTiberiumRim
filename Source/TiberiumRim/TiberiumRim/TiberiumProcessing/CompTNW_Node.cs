using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TiberiumRim
{
    public class CompTNW_Node : CompTNW
    {
        public override Color[] ColorOverrides => new Color[] { Network.GeneralColor, Color.white };
    }
}
