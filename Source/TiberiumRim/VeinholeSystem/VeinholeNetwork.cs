using System.Collections.Generic;
using RimWorld;
using Verse;

namespace TiberiumRim;

public class VeinholeNetwork
{
    public Veinhole parent;
    public List<VeinHub> hubs;
    public List<VeinEgg> eggs;
    public List<VeinMonster> pawns;

    
    private const float MinSpreadMass = 512;
    private float internalMass;

    public int CurrentMaxEggs => parent.TiberiumField.Area.Count / 64;
    public int CurrentMaxPawns => parent.TiberiumField.Area.Count / 32;
    
    public bool CanMakeEgg => eggs.Count + pawns.Count < CurrentMaxEggs + CurrentMaxPawns; 
    
    public VeinholeNetwork(Veinhole networkParent)
    {
        parent = networkParent;
        hubs = new List<VeinHub>();
        eggs = new List<VeinEgg>();
        pawns = new List<VeinMonster>();
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
        
    }

    public void TrySpawnEgg()
    {
        var randCell = parent.FieldCells.RandomElement();
        
    }
    
    public void Notify_Consumed(WrappedCorpse corpse)
    {
        internalMass += corpse.InnerPawn.GetStatValue(StatDefOf.MeatAmount) * 8;
    }
}