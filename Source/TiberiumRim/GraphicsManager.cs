using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace TiberiumRim
{
    public class GraphicsManager : GameComponent
    {
        public GraphicsManager()
        {
        }

        public GraphicsManager(Game game)
        {
        }

        public static GraphicsManager Manager
        {
            get
            {
                return Current.Game.GetComponent<GraphicsManager>();
            }
        }

        public bool CanGlow
        {
            get
            {
                return TRUtils.TiberiumSettings().graphicsSettings.TiberiumGlow;
            }
        }
    }
}
