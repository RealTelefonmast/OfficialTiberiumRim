using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleCore.Static;
using Verse;

namespace TeleCore
{
    //Defines the logical ruleset for a network
    public class NetworkDef : Def
    {
        //Cached Data
        [Unsaved]
        private Graphic_LinkedNetworkStructure cachedTransmitterGraphic;
        [Unsaved]
        private Graphic_Linked_NetworkStructureOverlay cachedOverlayGraphic;
        [Unsaved]
        private readonly List<NetworkValueDef> belongingValueDefs = new();

        //General Label
        public string containerLabel;

        // Loaded from XML
        public ThingDef portableContainerDef = TeleDefOf.PortableContainer;

        //
        public GraphicData transmitterGraphic;
        public GraphicData overlayGraphic;

        //Structure Ruleset
        public ThingDef controllerDef;
        public ThingDef transmitterDef;
        

        public bool UsesController => controllerDef != null;
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

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var configError in base.ConfigErrors())
            {
                yield return configError;
            }
            if (controllerDef != null)
            {
                var compProps = controllerDef.GetCompProperties<CompProperties_NetworkStructure>();
                if (compProps == null)
                {
                    yield return $"controllerDef {controllerDef} does not have a Network ThingComp!";
                }
                else if (compProps.networks.NullOrEmpty())
                {
                    yield return $"controllerDef {controllerDef} has no networks defined!";
                }
                else if (!compProps.networks.Find(n => n.networkDef == this).NetworkRole.HasFlag(NetworkRole.Controller))
                {
                    yield return $"controllerDef {controllerDef} does not have the Controller NetworkRole assigned!";
                }
            }
        }

        internal void Notify_ResolvedNetworkValueDef(NetworkValueDef networkValueDef)
        {
            belongingValueDefs.Add(networkValueDef);
        }
    }
}
