using System;
using System.Collections.Generic;
using TeleCore.Atmosphere.Defs;
using UnityEngine;
using Verse;

namespace TeleCore.Atmosphere.Grid;

public class SpreadingGasTypeDef : Def
{
    private static ushort _masterID;
    private static readonly Dictionary<int, SpreadingGasTypeDef> _defByID = new();

    [Unsaved]
    public ushort IDReference;

    //public string texPath;
    //public ShaderTypeDef shaderType;
    public Color colorMin;
    public Color colorMax;

    public int maxDensityPerCell = 100;
    public int minDissipationDensity = 10;
    public int minSpreadDensity = 2;
    public int dissipationAmount = 1;
    public float spreadViscosity = 0.35f;

    public AtmosphericValueDef dissipateTo;

    public FloatRange rotationSpeeds = new FloatRange(-100,100);
    public float accuracyPenalty;
    public bool blockTurretTracking;
    public bool roofBlocksDissipation = true;

    public int cellsToDissipatePerTick = 8;
    public int cellsToSpreadPerTick = 8;

    //
    public Type pawnEffectWorker;
    public Type cellEffectWorker;

    public float ViscosityMultiplier { get; private set; }

    public static implicit operator ushort(SpreadingGasTypeDef def) => def.IDReference;
    public static explicit operator SpreadingGasTypeDef(int ID) => _defByID[ID];

    public override IEnumerable<string> ConfigErrors()
    {
        foreach (var error in base.ConfigErrors())
        {
            yield return error;
        }

        if (maxDensityPerCell > ushort.MaxValue)
        {
            yield return $"{nameof(maxDensityPerCell)} cannot be larger than {ushort.MaxValue}!";
        }
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
