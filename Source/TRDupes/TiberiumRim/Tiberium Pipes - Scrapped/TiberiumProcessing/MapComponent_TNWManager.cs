using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace TiberiumRim
{
    public class MapComponent_TNWManager : MapComponent
    {
        public List<TiberiumNetwork> Networks = new List<TiberiumNetwork>();
        public int MasterID = -1;

        //Debug
        public static bool ShowNetworks = true;

        public MapComponent_TNWManager(Map map) : base(map)
        {
            
        }

        public override void MapComponentUpdate()
        {
            base.MapComponentUpdate();
            int i = 0;

            if (ShowNetworks)
            {
                foreach (TiberiumNetwork network in Networks)
                {
                    GenDraw.DrawFieldEdges(network.NetworkSet.FullList.SelectMany(b => b.OccupiedRect().Cells).ToList(), ColorByNum(i));
                    i++;
                }
            }
        }

        private Color ColorByNum(int num)
        {
            switch (num)
            {
                case 0:
                    return Color.blue;
                case 1:
                    return Color.cyan;
                case 2:
                    return Color.green;
                case 3:
                    return Color.magenta;
                case 4:
                    return Color.red;
                case 5:
                    return Color.yellow;
            }
            return Color.white;
        }

        public void RegisterNetwork(TiberiumNetwork tnw)
        {
            tnw.NetworkID = MasterID += 1;
            Networks.Add(tnw);
            //Log.Message("Registering ID " + tnw.NetworkID);
        }

        public void DeregisterNetwork(TiberiumNetwork tnw)
        {
            //Log.Message("Deregistering ID " + tnw?.NetworkID);
            Networks.Remove(tnw);
        }

        public List<TNW_Refinery> AllRefineries
        {
            get
            {
                return Networks.SelectMany(n => n.NetworkSet.Refineries) as List<TNW_Refinery>;
            }
        }
    }
}
