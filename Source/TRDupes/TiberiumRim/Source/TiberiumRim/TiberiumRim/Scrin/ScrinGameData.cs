using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class ScrinGameData
    {
        public int startingCredits = 10000;

        public int drones = 3;

        public int seeds = 1;

        public int CalculateCredits()
        {
            int creds = startingCredits;
            creds -= drones * 750;
            creds -= seeds * 2000;
            return creds;
        }
    }
}
