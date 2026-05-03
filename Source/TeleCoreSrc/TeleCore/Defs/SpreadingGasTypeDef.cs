using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TeleCore.Defs;

public class SpreadingGasTypeDef : Def
{
    private static ushort _masterID;
    private static readonly Dictionary<int, SpreadingGasTypeDef> _defByID = new();

    public float accuracyPenalty;
    public bool blockTurretTracking;
    public Type cellEffectWorker;

    public int cellsToDissipatePerTick = 8;
    public int cellsToSpreadPerTick = 8;
    public Color colorMax;

    public Color colorMin;

    public AtmosphericValueDef dissipateTo;
    public int dissipationAmount = 1;

    //public string texPath;
    //public ShaderTypeDef shaderType;

    [Unsaved] public ushort IDReference;

    public int maxDensityPerCell = 100;
    public int minDissipationDensity = 10;
    public int minSpreadDensity = 2;

    //
    public Type pawnEffectWorker;
    public bool roofBlocksDissipation = true;

    public FloatRange rotationSpeeds = new(-100, 100);
    public float spreadViscosity = 0.35f;

    public float ViscosityMultiplier { get; private set; }

    public static implicit operator ushort(SpreadingGasTypeDef def)
    {
        return def.IDReference;
    }

    public static explicit operator SpreadingGasTypeDef(int ID)
    {
        return _defByID[ID];
    }

    public override IEnumerable<string> ConfigErrors()
    {
        foreach (var error in base.ConfigErrors())
            yield return error;

        if (maxDensityPerCell > ushort.MaxValue)
            yield return $"{nameof(maxDensityPerCell)} cannot be larger than {ushort.MaxValue}!";
    }

    public override void PostLoad()
    {
        base.PostLoad();
        IDReference = _masterID++;
        _defByID.Add(IDReference, this);

        //
        ViscosityMultiplier = Mathf.Lerp(1, 0.0125f, spreadViscosity);
    }
}