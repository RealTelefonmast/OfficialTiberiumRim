using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;

namespace TiberiumRim
{
    public class ITab_TBills : ITab_Bills
    {
        private static readonly Vector2 WinSize = new Vector2(800, 500);

        public ITab_TBills()
        {
            this.size = WinSize;
            this.labelKey = "TR_TibResourceRefiner";
        }

        public override void FillTab()
        {

        }

        private void BillReadOut()
        {

        }

        private void BillSelectionCustom()
        {

        }
    }
}
