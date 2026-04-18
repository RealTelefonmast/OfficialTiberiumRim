using UnityEngine;
using Verse;

namespace TeleCore.Defs;

public class ParticleSystemDef : Def
{
}

//Modules 
public class ShapeProperties
{
    public int angle;
    public float arc = 0;
    public ParticleSystemShapeMultiModeValue arcMode = ParticleSystemShapeMultiModeValue.Random;
    public float arcSpread = 1;
    public float radius;
    public float radiusThickness;
    public ParticleSystemShapeType shape;

    public ParticleSystem.ShapeModule ShapeModuleFromDef()
    {
        var shape = new ParticleSystem.ShapeModule();
        shape.radius = 1;
        return shape;
    }
}

public class EmissionProperties
{
    public int burstCount;


    public ParticleSystem.EmissionModule EmissionModuleFromDef()
    {
        var emission = new ParticleSystem.EmissionModule();
        emission.burstCount = burstCount;
        return emission;
    }
}