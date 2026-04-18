// Preserved from TeleCore/SpreadingGas/SpreadingGasTypeDef.cs

using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TeleCore.Defs;

public class SpreadingGasTypeDef_TAE : Def
{
    private static ushort _masterID;
    private static readonly Dictionary<int, SpreadingGasTypeDef_TAE> _defByID = new();

    [Unsaved] private AtmosphericTransferWorker workerInt;
    [Unsaved] public ushort IDReference;

    public Color colorMin;
    public Color colorMax;

    public int maxDensityPerCell = 100;
    public int minDissipationDensity = 10;
    public int minSpreadDensity = 2;
    public int dissipationAmount = 1;

    //
    public Type pawnEffectWorker;
    public bool roofBlocksDissipation = true;

    public FloatRange rotationSpeeds = new(-100, 100);
    public float spreadViscosity = 0;

    public TAE.AtmosphericDef dissipateTo;
    public Type transferWorker = typeof(TAE.AtmosphericTransferWorker);

    public FloatRange rotationSpeeds = new(-100, 100);
    public float accuracyPenalty;
    public bool blockTurretTracking;
    public bool roofBlocksDissipation = true;

    public int cellsToDissipatePerTick = 8;
    public int cellsToSpreadPerTick = 8;

    //
    public Type pawnEffectWorker;
    public Type cellEffectWorker;

    public TAE.AtmosphericTransferWorker TransferWorker =>
        workerInt ??= (TAE.AtmosphericTransferWorker)Activator.CreateInstance(transferWorker, this);

    public float ViscosityMultiplier { get; private set; }

    public static implicit operator ushort(SpreadingGasTypeDef_TAE def)
    {
        return def.IDReference;
    }

    public static explicit operator SpreadingGasTypeDef_TAE(int ID)
    {
        return _defByID[ID];
    }

    public override IEnumerable<string> ConfigErrors()
    {
        foreach (var error in base.ConfigErrors()) yield return error;

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