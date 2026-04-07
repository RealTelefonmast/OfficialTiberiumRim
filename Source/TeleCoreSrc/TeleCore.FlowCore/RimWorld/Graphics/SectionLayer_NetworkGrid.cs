using System.Collections.Generic;
using RimWorld;
using TeleCore.FlowCore;
using UnityEngine;
using Verse;
using NetworkDef = TeleCore.RimWorld.Defs.NetworkDef;

namespace TeleCore.RimWorld.Graphics;

public class SectionLayer_NetworkGrid : SectionLayer_Things
{
    internal static NetworkDef CURRENT_NETWORK;
    private readonly Dictionary<NetworkDef, List<LayerSubMesh>> SubmeshesByNetwork = new();

    public SectionLayer_NetworkGrid(Section section) : base(section)
    {
        requireAddToMapMesh = false;
        relevantChangeTypes = MapMeshFlagDefOf.Buildings;
    }

    public static IEnumerable<NetworkDef> NetworksFromDesignator(Designator designator)
    {
        if (designator is not Designator_Build build) yield break;
        if (((build.PlacingDef as ThingDef)?.comps is not { } comps)) yield break;

        foreach (var comp in comps)
        {
            if (comp is not CompProperties_Network network) continue;
            foreach (var config in network.networks)
            {
                yield return config.networkDef;
            }
        }
    }

    private void DrawSubLayer(NetworkDef def)
    {
        if (!Visible) return;
        List<LayerSubMesh> subMeshes;

        if (!SubmeshesByNetwork.TryGetValue(def, out subMeshes)) return;
        var count = subMeshes.Count;
        for (var i = 0; i < count; i++)
        {
            var layerSubMesh = subMeshes[i];
            if (layerSubMesh.finalized && !layerSubMesh.disabled)
                UnityEngine.Graphics.DrawMesh(layerSubMesh.mesh, Matrix4x4.identity, layerSubMesh.material, 0);
        }
    }

    public override void DrawLayer()
    {
        if (Find.DesignatorManager.SelectedDesignator is Designator_Build designator)
        {
            foreach (var def in NetworksFromDesignator(designator))
            {
                DrawSubLayer(def);
            }
        }
    }

    protected void ClearSubMeshesFull(MeshParts parts)
    {
        foreach (var subMeshList in SubmeshesByNetwork)
        {
            if (subMeshList.Value.NullOrEmpty()) continue;
            foreach (var subMesh in subMeshList.Value) subMesh.Clear(parts);
        }
    }

    protected void FinalizeMeshFull(MeshParts tags)
    {
        foreach (var subMeshList in SubmeshesByNetwork)
        {
            if (subMeshList.Value.NullOrEmpty()) continue;
            subMeshList.Value.RemoveAll(t => t.verts.Count == 0 || t.tris.Count == 0);
            foreach (var subMesh in subMeshList.Value) subMesh.FinalizeMesh(tags);
        }
    }

    public override void Regenerate()
    {
        ClearSubMeshesFull(MeshParts.All);
        foreach (var item in section.CellRect)
        {
            List<Thing> list = Map.thingGrid.ThingsListAt(item);
            var count = list.Count;
            for (var i = 0; i < count; i++)
            {
                var thing = list[i];
                if ((thing.def.seeThroughFog ||
                     !Map.fogGrid.fogGrid[CellIndicesUtility.CellToIndex(thing.Position, Map.Size.x)]) &&
                    thing.def.drawerType != 0 &&
                    (thing.def.drawerType != DrawerType.RealtimeOnly || !requireAddToMapMesh) &&
                    (!(thing.def.hideAtSnowOrSandDepth < 1f) ||
                     !(Map.snowGrid.GetDepth(thing.Position) > thing.def.hideAtSnowOrSandDepth)) &&
                    thing.Position.x == item.x && thing.Position.z == item.z) TakePrintFrom(thing);
            }
        }

        FinalizeMeshFull(MeshParts.All);
    }

    public LayerSubMesh GetSubMeshCurrent(Material material)
    {
        if (material == null) return null;

        List<LayerSubMesh> subMeshes;
        if (!SubmeshesByNetwork.TryGetValue(CURRENT_NETWORK, out subMeshes))
        {
            subMeshes = new List<LayerSubMesh>();
            SubmeshesByNetwork.Add(CURRENT_NETWORK, subMeshes);
        }
        else
        {
            for (var i = 0; i < subMeshes.Count; i++)
                if (subMeshes[i].material == material)
                    return subMeshes[i];
        }

        var mesh = new Mesh();
        var value = new Bounds(section.botLeft.ToVector3(), Vector3.zero);
        value.Encapsulate(section.botLeft.ToVector3() + new Vector3(17f, 0f, 0f));
        value.Encapsulate(section.botLeft.ToVector3() + new Vector3(17f, 0f, 17f));
        value.Encapsulate(section.botLeft.ToVector3() + new Vector3(0f, 0f, 17f));
        var layerSubMesh = new LayerSubMesh(mesh, material, value);
        SubmeshesByNetwork[CURRENT_NETWORK].Add(layerSubMesh);
        return layerSubMesh;
    }

    public override void TakePrintFrom(Thing t)
    {
        var comp = t.TryGetComp<CompNetwork>();
        if (comp == null) return;
        foreach (var networkComponent in comp.NetworkParts)
        {
            CURRENT_NETWORK = networkComponent.Config.networkDef;
            CURRENT_NETWORK.OverlayGraphic?.Print(this, t, 0);
            CURRENT_NETWORK = null;
        }
    }
}