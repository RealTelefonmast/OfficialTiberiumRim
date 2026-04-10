using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;

namespace TiberiumRim
{ 
    public class ITab_MechUpgrade : ITab
    {
        private MechRecipeDef selectedRecipe;
        private float viewHeight = 1000f;
        private static readonly Vector2 WinSize = new Vector2(420f, 480f);
        private static Vector2 BPWinSize = new Vector2(275, 275);
        private static Vector2 BPSize = new Vector2(200, 200);
        private Vector2 scrollPosition = default(Vector2);


        public ITab_MechUpgrade()
        {
            this.size = WinSize;
            this.labelKey = "TabMechsUpgrade";
            //this.blueprint = new MechBlueprint("Pawns/Common/Harvester/Blueprint/Harvester");
        }

        protected override void FillTab()
        {

        }
    }
}
