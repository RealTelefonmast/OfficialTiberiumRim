using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim;

public class TiberiumLeak : Filth
{
    private Color color;

    public override Color DrawColor => color;

    public override Color DrawColorTwo => color;

    public override Graphic Graphic
    {
        get
        {
            var graphic = base.Graphic;
            graphic.color = color;
            graphic.colorTwo = color;
            return graphic;
        }
    }

    public void Setup(Color color)
    {
        this.color = color;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref color, "color");
    }

    public override void Tick()
    {
        base.Tick();
    }
}