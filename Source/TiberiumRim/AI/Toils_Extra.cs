using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace TR;

public static class Toils_Extra
{
	public static Toil CarryHauledThingToCell(LocalTargetInfo targetInfo, PathEndMode pathEndMode = PathEndMode.ClosestTouch)
	{
		Toil toil = new Toil();
		toil.initAction = delegate ()
		{
			IntVec3 cell = targetInfo.Cell;
			toil.actor.pather.StartPath(cell, pathEndMode);
		};
		toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
		toil.AddFailCondition(delegate
		{
			Pawn actor = toil.actor;
			IntVec3 cell = targetInfo.Cell;
			return actor.jobs.curJob.haulMode == HaulMode.ToCellStorage && !cell.IsValidStorageFor(actor.Map, actor.carryTracker.CarriedThing);
		});
		return toil;
	}

	public static Toil PlaceHauledThingInCell(LocalTargetInfo targetInfo, Toil nextToilOnPlaceFailOrIncomplete, bool storageMode, bool tryStoreInSameStorageIfSpotCantHoldWholeStack = false)
	{
		Toil toil = new Toil();
		toil.initAction = delegate ()
		{
			Pawn actor = toil.actor;
			Job curJob = actor.jobs.curJob;
			IntVec3 cell = targetInfo.Cell;
			if (actor.carryTracker.CarriedThing == null)
			{
				Log.Error(actor + " tried to place hauled thing in cell but is not hauling anything.");
				return;
			}
			SlotGroup slotGroup = actor.Map.haulDestinationManager.SlotGroupAt(cell);
			if (slotGroup != null && slotGroup.Settings.AllowedToAccept(actor.carryTracker.CarriedThing))
			{
				actor.Map.designationManager.TryRemoveDesignationOn(actor.carryTracker.CarriedThing, DesignationDefOf.Haul);
			}
			Action<Thing, int> placedAction = null;
			if (curJob.def == JobDefOf.DoBill || curJob.def == JobDefOf.RecolorApparel || curJob.def == JobDefOf.RefuelAtomic || curJob.def == JobDefOf.RearmTurretAtomic)
			{
				placedAction = delegate (Thing th, int added)
				{
					if (curJob.placedThings == null)
					{
						curJob.placedThings = new List<ThingCountClass>();
					}
					ThingCountClass thingCountClass = curJob.placedThings.Find((ThingCountClass x) => x.thing == th);
					if (thingCountClass != null)
					{
						thingCountClass.Count += added;
						return;
					}
					curJob.placedThings.Add(new ThingCountClass(th, added));
				};
			}
			Thing thing;
			if (!actor.carryTracker.TryDropCarriedThing(cell, ThingPlaceMode.Direct, out thing, placedAction))
			{
				if (storageMode)
				{
					IntVec3 c;
					if (nextToilOnPlaceFailOrIncomplete != null && ((tryStoreInSameStorageIfSpotCantHoldWholeStack && StoreUtility.TryFindBestBetterStoreCellForIn(actor.carryTracker.CarriedThing, actor, actor.Map, StoragePriority.Unstored, actor.Faction, cell.GetSlotGroup(actor.Map), out c, true)) || StoreUtility.TryFindBestBetterStoreCellFor(actor.carryTracker.CarriedThing, actor, actor.Map, StoragePriority.Unstored, actor.Faction, out c, true)))
					{
						if (actor.CanReserve(c, 1, -1, null, false))
						{
							actor.Reserve(c, actor.CurJob, 1, -1, null, true);
						}
						actor.CurJob.SetTarget(TargetIndex.B, c);
						actor.jobs.curDriver.JumpToToil(nextToilOnPlaceFailOrIncomplete);
						return;
					}
					IntVec3 c2;
					if (HaulAIUtility.CanHaulAside(actor, actor.carryTracker.CarriedThing, out c2))
					{
						curJob.SetTarget(TargetIndex.B, c2);
						curJob.count = int.MaxValue;
						curJob.haulOpportunisticDuplicates = false;
						curJob.haulMode = HaulMode.ToCellNonStorage;
						actor.jobs.curDriver.JumpToToil(nextToilOnPlaceFailOrIncomplete);
						return;
					}
					Log.Warning(string.Format("Incomplete haul for {0}: Could not find anywhere to put {1} near {2}. Destroying. This should be very uncommon!", actor, actor.carryTracker.CarriedThing, actor.Position));
					actor.carryTracker.CarriedThing.Destroy(DestroyMode.Vanish);
					return;
				}
				else if (nextToilOnPlaceFailOrIncomplete != null)
				{
					actor.jobs.curDriver.JumpToToil(nextToilOnPlaceFailOrIncomplete);
					return;
				}
			}
		};
		return toil;
	}

	public static Toil CarryHauledThingToContainer(LocalTargetInfo targetInfo)
	{
		Toil gotoDest = new Toil();
		gotoDest.initAction = delegate ()
		{
			gotoDest.actor.pather.StartPath(targetInfo.Thing, PathEndMode.Touch);
		};
		gotoDest.AddFailCondition(delegate
		{
			Thing thing = targetInfo.Thing;
			if (thing.Destroyed || (!gotoDest.actor.jobs.curJob.ignoreForbidden && thing.IsForbidden(gotoDest.actor)))
			{
				return true;
			}
			ThingOwner thingOwner = thing.TryGetInnerInteractableThingOwner();
			return thingOwner != null && !thingOwner.CanAcceptAnyOf(gotoDest.actor.carryTracker.CarriedThing, true);
		});
		gotoDest.defaultCompleteMode = ToilCompleteMode.PatherArrival;
		return gotoDest;
	}
}