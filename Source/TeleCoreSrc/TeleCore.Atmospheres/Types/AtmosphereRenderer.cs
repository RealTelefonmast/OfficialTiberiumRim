using System;
using System.Collections.Generic;
using System.Linq;
using TeleCore.Defs;
using TeleCore.RoomComponents;
using TeleCore.Types.Utils;
using UnityEngine;
using Verse;

namespace TeleCore.Atmospheres.Types;

public class AtmosphereRenderer
{
    private readonly List<AtmosphericValueDef> atmospheres;
    private readonly Map map;
    private readonly List<SkyOverlay> naturalOverlays = new();

    private CellBoolDrawer drawerInt;

    private AtmosphericValueDef selectedAtmosphere;

    public AtmosphereRenderer(Map map)
    {
        this.map = map;
        atmospheres = DefDatabase<AtmosphericValueDef>.AllDefs.Where(t => t.useRenderLayer).ToList();
    }

    private CellBoolDrawer Drawer => drawerInt ??= new CellBoolDrawer(CellBoolDrawerGetBoolInt, CellBoolDrawerColorInt,
        CellBoolDrawerGetExtraColorInt, map.Size.x, map.Size.z, 3610);

    private float AtmosphereAt(IntVec3 loc)
    {
        return CalculateAtmosphereAt(loc);
    }

    public float CalculateAtmosphereAt(IntVec3 loc, AtmosphericValueDef valueDef = null)
    {
        var room = loc.GetRoomFast(map);
        var roomComp = room?.GetRoomComp<RoomComponent_Atmosphere>();
        if (roomComp != null) return roomComp.Volume.StoredPercentOf(valueDef ?? selectedAtmosphere);
        return 0;
    }

    public void AtmosphereDrawerUpdate()
    {
        if (false) // TODO: wire up to TeleCore atmosphere settings once AtmosphereMod is replaced
        {
            Drawer.MarkForDraw();
            Drawer.CellBoolDrawerUpdate();
        }
    }

    public void Drawer_SetDirty()
    {
        Drawer.SetDirty();
    }

    private bool CellBoolDrawerGetBoolInt(int index)
    {
        var intVec = CellIndicesUtility.IndexToCell(index, map.Size.x);
        return !intVec.Filled(map) && !intVec.Fogged(map) && AtmosphereAt(intVec) > 0.69f;
    }

    private Color CellBoolDrawerColorInt()
    {
        return Color.white;
    }

    public Color CellBoolDrawerGetExtraColorInt(int index)
    {
        return Color.Lerp(Color.clear, selectedAtmosphere.valueColor,
            AtmosphereAt(CellIndicesUtility.IndexToCell(index, map.Size.x)));
    }

    public Color CellBoolDrawerGetExtraColorInt(int index, AtmosphericValueDef valueDef)
    {
        return Color.Lerp(Color.clear, valueDef.valueColor,
            AtmosphereAt(CellIndicesUtility.IndexToCell(index, map.Size.x)));
    }

    //Selection
    internal void OpenAtmosphereLayerMenu(Action<bool> callback)
    {
        var options = new List<FloatMenuOption>();
        foreach (var atmosphericDef in atmospheres)
            options.Add(new FloatMenuOption(atmosphericDef.LabelCap, delegate
            {
                var diff = atmosphericDef != selectedAtmosphere;
                callback.Invoke(diff);
                if (diff)
                    selectedAtmosphere = atmosphericDef;
                else
                    selectedAtmosphere = null;
            }));

        Find.WindowStack.Add(new FloatMenu(options, "TAE_SelectLayer".Translate()));
    }

    private void DrawSkyOverlays()
    {
        if (naturalOverlays.NullOrEmpty()) return;
        for (var i = 0; i < naturalOverlays.Count; i++) naturalOverlays[i].DrawOverlay(map);
    }

    internal void Draw()
    {
        DrawSkyOverlays();
    }

    public void Tick()
    {
        foreach (var overlay in naturalOverlays)
            //TODO: create config to get color
            //overlay.OverlayColor = naturalAtmospheres[0].Def.valueColor;
            overlay.TickOverlay(map);
    }
}