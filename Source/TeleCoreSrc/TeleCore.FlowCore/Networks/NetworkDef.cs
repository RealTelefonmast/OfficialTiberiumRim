using System.Collections.Generic;
using Verse;

namespace TeleCore.FlowCore;

//Defines the logical ruleset for a network
public class NetworkDef : FlowValueCollectionDef
{
    [Unsaved]
    private Graphic_Linked_NetworkStructureOverlay cachedOverlayGraphic;

    //Cached Data
    [Unsaved]
    private Graphic_LinkedNetworkStructure cachedTransmitterGraphic;

    //General Label
    public string containerLabel;

    //Structure Ruleset
    public ThingDef? controllerDef;
    public ThingDef? transmitterDef;
    public string labelShort;
    public GraphicData overlayGraphic;

    public float? gasThroughPutOverride;
    public float? frictionOverride;

    //
    public ThingDef portableContainerDef = TeleDefOf.PortableContainer;

    //
    public GraphicData transmitterGraphic;

    public bool UsesController => controllerDef != null;

    public Graphic_LinkedNetworkStructure TransmitterGraphic
    {
        get { return cachedTransmitterGraphic ??= new Graphic_LinkedNetworkStructure(transmitterGraphic.Graphic); }
    }

    public Graphic_Linked_NetworkStructureOverlay OverlayGraphic
    {
        get { return cachedOverlayGraphic ??= new Graphic_Linked_NetworkStructureOverlay(overlayGraphic.Graphic); }
    }

    public override IEnumerable<string> ConfigErrors()
    {
        foreach (var configError in base.ConfigErrors()) yield return configError;

        if (labelShort == null) labelShort = label ?? defName.Substring(0, 2);

        if (controllerDef != null)
        {
            var compProps = controllerDef.GetCompProperties<CompProperties_Network>();
            if (compProps == null)
            {
                yield return $"controllerDef {controllerDef} does not have a Network ThingComp!";
            }
            else if (compProps.networks.NullOrEmpty())
            {
                yield return $"controllerDef {controllerDef} has no networks defined!";
            }
            else
            {
                var networkDef = compProps.networks.Find(n => n.networkDef == this);
                if (networkDef == null)
                    yield return
                        $"Network seems unused: Cannot find any {nameof(NetworkPartConfig)} using this network.";
                else if ((networkDef.roles & NetworkRole.Controller) != NetworkRole.Controller)
                    yield return $"controllerDef {controllerDef} does not have the Controller NetworkRole assigned!";
            }
        }
    }
}