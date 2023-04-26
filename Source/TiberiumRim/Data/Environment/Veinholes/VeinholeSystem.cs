using System;
using System.Collections.Generic;
using RimWorld;
using TeleCore;
using TiberiumRim.Data.Enums;
using Verse;

namespace TiberiumRim;

public class VeinholeSystem : IExposable
{
    //
    private const float MinSpreadMass = 512;
    
    // 
    public Veinhole parent;
    public List<VeinHub> hubs;
    public List<VeinEgg> eggs;
    public List<VeinRoamer> pawns;

    private float internalMass;
    private int[] tickers;
    
    public int CurrentMaxEggs => parent.TiberiumField.Area.Count / 64;
    public int CurrentMaxPawns => parent.TiberiumField.Area.Count / 32;
    
    public bool CanMakeEgg => eggs.Count + pawns.Count < CurrentMaxEggs + CurrentMaxPawns;
    public bool IsAlive => !parent.Destroyed;

    public VeinholeSystem(Veinhole networkParent)
    {
        parent = networkParent;
        hubs = new List<VeinHub>();
        eggs = new List<VeinEgg>();
        pawns = new List<VeinRoamer>();
        tickers = new int[2];
    }

    public void ExposeData()
    {
        Scribe_Arrays.Look(ref tickers, "tickers");
    }
    
    public void Init()
    {
        
    }
    
    public void Tick()
    {
        //Spread Hub
        if (Rand.MTBEventOccurs(2, GenDate.TicksPerDay, 1))
        {
            TrySpreadHub();
        }
        
        //Spread Egg
        if (CanMakeEgg && Rand.MTBEventOccurs(1, GenDate.TicksPerDay, 1))
        {
            TrySpawnEgg();
        }
    }

    public bool Notify_RequestSpread()
    {
        if (internalMass >= MinSpreadMass)
        {
            internalMass -= 32;
            return true;
        }
        return false;
    }

    public void TrySpreadHub()
    {
        if (tickers[0] != 0) return;

        Action<IntVec3> Processor = delegate(IntVec3 c)
        {
            TerrainDef terrain = parent.Ruleset.RandomOutcome(c.GetTerrain(parent.Map));
            if (terrain != null)
                parent.Map.terrainGrid.SetTerrain(c, terrain);
        };

        IntVec3 end = GenRadial.RadialCellsAround(parent.Position, 56, false).RandomElement();
        _ = TeleFlooder.TryMakeConnection(parent.Position, end, Processor);
        var hub = (VeinHub)GenSpawn.Spawn(ThingDef.Named("VeinHub"), end, parent.Map);
        hubs.Add(hub);

        ResetTimer(0);   
    }

    public void TrySpawnEgg()
    {
        var randCell = parent.FieldCells.RandomElement();
        if (tickers[1] != 0) return;

        var cell = parent.FieldCells.RandomElement();
        GenSpawn.Spawn(ThingDef.Named("VeinEgg"), cell, parent.Map);

        ResetTimer(1);
    }

    private void ResetTimer(int i)
    {
        if(i == 0)
            tickers[0] = (int)(GenDate.TicksPerDay * TRandom.Range(3f, 7f));
        if(i == 1)
            tickers[1] = (int)(GenDate.TicksPerDay * TRandom.Range(1f, 3f));
    }

    public void Notify_Consumed(WrappedCorpse corpse)
    {
        internalMass += corpse.InnerPawn.GetStatValue(StatDefOf.MeatAmount) * 8;
    }

    public void AddPart(Thing part, VeinholeSystemType partType)
    {
        switch (partType)
        {
            case VeinholeSystemType.Hub:
                hubs.Add((VeinHub)part);
                break;
            case VeinholeSystemType.Egg:
                eggs.Add((VeinEgg)part);
                break;
            case VeinholeSystemType.Roamer:
                pawns.Add((VeinRoamer)part);
                break;
        }
    }
    
    public void RemovePart(Thing part, VeinholeSystemType partType)
    {
        switch (partType)
        {
            case VeinholeSystemType.Hub:
                hubs.Remove((VeinHub)part);
                break;
            case VeinholeSystemType.Egg:
                eggs.Remove((VeinEgg)part);
                break;
            case VeinholeSystemType.Roamer:
                pawns.Remove((VeinRoamer)part);
                break;
        }
    }
}