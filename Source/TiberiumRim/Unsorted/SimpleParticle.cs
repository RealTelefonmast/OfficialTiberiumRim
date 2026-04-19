using UnityEngine;
using Verse;

namespace TiberiumRim;

public class SimpleParticle
{
    public ParticleDef def;
    private IntVec3 endCell = IntVec3.Invalid;

    private float exactScale = 1f;

    private Graphic graphicInt;
    private IntVec3 startCell = IntVec3.Invalid;


    public SimpleParticle()
    {
    }

    public SimpleParticle(ParticleDef def)
    {
        this.def = def;
    }

    public float ExactRotation { get; } = 0f;

    public Vector3 ExactScale { get; } = Vector3.one;

    public Vector3 Position { get; }

    public Graphic Graphic => graphicInt ??= def.graphicData.Graphic;

    public void SpawnSetup()
    {
    }

    public virtual void Tick()
    {
    }

    public void Draw()
    {
    }
}