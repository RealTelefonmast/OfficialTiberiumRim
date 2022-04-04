using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    //Defines the logical ruleset for a network
    public class NetworkDef : Def
    {
        //Cached Data
        private Graphic_LinkedNetworkStructure cachedTransmitterGraphic;
        private Graphic_Linked_NetworkStructureOverlay cachedOverlayGraphic;
        private List<NetworkValueDef> belongingValueDefs = new List<NetworkValueDef>();

        /// Loaded from XML ///
        public string containerLabel;

        //
        public GraphicData transmitterGraphic;
        public GraphicData overlayGraphic;

        //Structure Ruleset
        public ThingDef controllerDef;
        public ThingDef transmitter;


        public List<NetworkValueDef> NetworkValueDefs => belongingValueDefs;

        public Graphic_LinkedNetworkStructure TransmitterGraphic
        {
            get
            {
                return cachedTransmitterGraphic ??= new Graphic_LinkedNetworkStructure(transmitterGraphic.Graphic);
            }
        }

        public Graphic_Linked_NetworkStructureOverlay OverlayGraphic
        {
            get
            {
                return cachedOverlayGraphic ??= new Graphic_Linked_NetworkStructureOverlay(overlayGraphic.Graphic);
            }
        }

        public void ResolvedValueDef(NetworkValueDef networkValueDef)
        {
            belongingValueDefs.Add(networkValueDef);
        }
    }
}
