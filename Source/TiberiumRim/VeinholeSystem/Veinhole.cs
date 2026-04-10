using System;
using System.Collections.Generic;
using TeleCore.ThingComps;
using Verse;

namespace TR;

public class Veinhole : TiberiumProducer
{
    private const int hubRadius = 70;

    //
    private Environment.Veinholes.VeinholeSystem _system;


    //
    public Comp_AnimationRenderer AnimationComp { get; private set; }
    public Environment.Veinholes.VeinholeSystem System => _system;

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        //
        _system = new Environment.Veinholes.VeinholeSystem(this);
        _system.Init();

        //Shake the camera!
        Find.CameraDriver.shaker.DoShake(0.2f);
        base.SpawnSetup(map, respawningAfterLoad);

        AnimationComp = GetComp<Comp_AnimationRenderer>();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Deep.Look(ref _system, "livingNetwork");
    }

    public override void Tick()
    {
        base.Tick();
        _system.Tick();
    }

    private void TryConsume(WrappedCorpse corpse)
    {
        _system.Notify_Consumed(corpse);
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var gizmo in base.GetGizmos()) yield return gizmo;

        yield return new Command_Action
        {
            defaultLabel = "Spawn Hub",
            action = () => _system.TrySpreadHub()
        };
    }
}