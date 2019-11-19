using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class Alert_TiberiumExposure : Alert
    {
        public override AlertReport GetReport()
        {
            return false;
        }
    }
}
