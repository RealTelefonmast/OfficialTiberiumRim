using System.Collections.Generic;
using System.Linq;
using RimWorld;
using TeleCore.Primitive;
using TR.Buildings;
using TR.Defs;
using TR.Util;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TR.Data;

public class MechConstructionBillStack
{
    private readonly Building_Hangar _parent;
    private MechConstructionBill _curItem;

    public MechConstructionBillStack(Building_Hangar parent)
    {
        _parent = parent;
        Queue = new List<MechConstructionBill>();
    }

    public bool HasBillWaiting => CurrentItem is { IsPreparing: true };
    public MechConstructionBill CurBill => CurrentItem;
    public IReadOnlyCollection<MechConstructionBill> All => Queue;
    internal List<MechConstructionBill> Queue { get; }

    public MechConstructionBill CurrentItem => Queue.Count > 0 ? Queue.First() : null;

    public void Tick()
    {
        if (Queue.Count <= 0) return;

        if (CurrentItem is { IsPreparing: false, IsFinished: false })
        {
            CurrentItem.TickProgress();
            return;
        }

        if (CurrentItem is { IsFinished: true })
            Finish();

        var item = Queue[0];
        if (item.TryStartNow())
            CurrentItem = item;
    }

    public void Begin(MechConstructionBill bill)
    {
    }

    public void Finish()
    {
        var item = CurrentItem;
        Queue.Remove(item);
        //Spawn mech
        PawnKindDef kind = item.Recipe.mechDef;
        var pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, _parent.Faction,
            PawnGenerationContext.NonPlayer, -1, false, false, true, true, false, 1f, false, true, false, true, true,
            false, false, false, false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null,
            null, null, false, false, false, false, null, null, null, null, null, 0f, DevelopmentalStage.Newborn));
        GenSpawn.Spawn(pawn, _parent.Position, _parent.Map);
        var freeSpot =
            CellFinder.RandomClosewalkCellNear(_parent.Position, _parent.Map, _parent.RotatedSize.MagnitudeManhattan);
        pawn.jobs.StartJob(new Job(JobDefOf.Goto, freeSpot), JobCondition.InterruptForced);
    }

    public void Notify_Reordered(MechRecipeDef def, int newIndex)
    {
        var bill = Queue.Find(c => c.Recipe == def);
        Queue.Remove(bill);
        Queue.Insert(newIndex, bill);
    }

    public void Add(MechRecipeDef recipe)
    {
        var bill = new MechConstructionBill(recipe);
        Queue.Add(bill);
    }

    public void AddRecipe(MechRecipeDef recipe)
    {
        Queue.Add(new MechConstructionBill(recipe));
    }

    public void Delete(MechConstructionBill bill)
    {
        Queue.Remove(bill);
        //TODO: Refund
    }

    public void Notify_AddedResources(Thing thing, int count)
    {
        if (thing.stackCount != count)
            TRLog.Warning(
                $"Something is wrong: {thing} has a stack count of {thing.stackCount} but we are trying to add {count} to the bill.");
        CurrentItem.Notify_ResourceAdded(thing, thing.stackCount);
    }
}

public class MechConstructionBill
{
    private DefValueStack<ThingDef, int> _inputCache;
    private bool _isActive;
    private bool _isPaid;
    private int _progress;

    private bool isSelected;

    public MechConstructionBill(MechRecipeDef recipeDef)
    {
        Recipe = recipeDef;
        _inputCache = new DefValueStack<ThingDef, int>();
    }

    public bool IsPreparing => MissingResources.Any();

    public float ProgressPercent => _progress / (float)Recipe.workCost;
    public float ItemProgress => _inputCache.TotalValue / (float)Recipe.costList.Sum(c => c.count);

    public string ProgressLabel => $"{_progress}/{Recipe.workCost}";
    public string ItemProgressLabel => $"{_inputCache.TotalValue}/{Recipe.costList.Sum(c => c.count)}";

    public MechRecipeDef Recipe { get; }

    public bool IsFinished => _progress >= Recipe.workCost;

    public IEnumerable<ThingDefCount> MissingResources
    {
        get
        {
            foreach (var needed in Recipe.costList)
            {
                var left = needed.count - _inputCache[needed.thingDef].Value;
                if (left > 0)
                    yield return new ThingDefCount(needed.thingDef, left);
            }
        }
    }

    public void Notify_Selected(bool selected)
    {
        isSelected = selected;
    }

    public void Notify_ResourceAdded(Thing thing, int count)
    {
        _inputCache += (thing.def, count);
    }

    public void TickProgress()
    {
        if (!IsFinished)
            _progress = Mathf.Clamp(_progress + 10, 0, Recipe.workCost);
    }


    public void AddInput(ThingDefCount input)
    {
    }

    public void CheckBeginNow()
    {
        _isPaid = Recipe.costList.All(cost =>
            _inputCache.Any(input => input.thingDef == cost.thingDef && input.Count >= cost.count));
    }

    public bool CanStartNow()
    {
        CheckBeginNow();
        return _isPaid;
    }

    public bool TryStartNow()
    {
        if (!CanStartNow()) return false;
        return _isActive = true;
    }
}