using System;
using System.Collections.Generic;
using System.Linq;
using TeleCore.Defs;
using TeleCore.FlowCore;
using Verse;

namespace TeleCore.Unsorted;

public abstract class ValueContainerWithHolderThing<TValue, THolder> : ValueContainerWithHolder<TValue, THolder>
    where TValue : FlowValueDef
    where THolder : IContainerHolderThing<TValue>
{
    private Gizmo_ContainerStorage? _gizmoInt;

    public ValueContainerWithHolderThing(ContainerConfig<TValue> config, THolder holder) : base(config, holder)
    {
    }

    public Gizmo_ContainerStorage ContainerGizmo => _gizmoInt ??= new Gizmo_ContainerStorage(this);

    public Thing ParentThing => Holder.Thing;

    public void Notify_ParentDestroyed(DestroyMode mode, Map previousMap)
    {
        if (TotalStored <= 0 || mode == DestroyMode.Vanish) return;
        OnParentDestroyed(mode, previousMap);
        Clear();
    }

    public virtual void OnParentDestroyed(DestroyMode mode, Map previousMap)
    {
        if (mode is DestroyMode.KillFinalize)
        {
            if (Config.explosionProps != null)
                if (TotalStored > 0)
                    //float radius = Props.explosionProps.explosionRadius * StoredPercent;
                    //int damage = (int)(10 * StoredPercent);
                    //var mainTypeDef = MainValueType.dropThing;
                    Config.explosionProps.DoExplosion(ParentThing.Position, previousMap, ParentThing);
            //GenExplosion.DoExplosion(Parent.Thing.Position, previousMap, radius, DamageDefOf.Bomb, Parent.Thing, damage, 5, null, null, null, null, mainTypeDef, 0.18f);
            if (Config.dropContents)
            {
                var i = 0;
                var drops = GetThingDrops().ToList();
                Predicate<IntVec3> pred = c => c.InBounds(previousMap) && c.GetEdifice(previousMap) == null;
                var action = delegate(IntVec3 c)
                {
                    if (i < drops.Count)
                    {
                        var drop = drops[i];
                        if (drop != null)
                        {
                            GenSpawn.Spawn(drop, c, previousMap);
                            drops.Remove(drop);
                        }

                        i++;
                    }
                };
                _ = TeleFlooder.Flood(previousMap, ParentThing.OccupiedRect(), action, pred, drops.Count);
            }
        }
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        if (Capacity <= 0) yield break;


        if (Holder.ShowStorageGizmo)
            if (Find.Selector.NumSelected == 1 && Find.Selector.IsSelected(ParentThing))
                yield return ContainerGizmo;

        /*
        if (DebugSettings.godMode)
        {
            yield return new Command_Action
            {
                defaultLabel = $"DEBUG: Container Options {Props.maxStorage}",
                icon = TiberiumContent.ContainMode_TripleSwitch,
                action = delegate
                {
                    List<FloatMenuOption> list = new List<FloatMenuOption>();
                    list.Add(new FloatMenuOption("Add ALL", delegate
                    {
                        foreach (var type in AcceptedTypes)
                        {
                            TryAddValue(type, 500, out _);
                        }
                    }));
                    list.Add(new FloatMenuOption("Remove ALL", delegate
                    {
                        foreach (var type in AcceptedTypes)
                        {
                            TryRemoveValue(type, 500, out _);
                        }
                    }));
                    foreach (var type in AcceptedTypes)
                    {
                        list.Add(new FloatMenuOption($"Add {type}", delegate
                        {
                            TryAddValue(type, 500, out var _);
                        }));
                    }
                    FloatMenu menu = new FloatMenu(list, $"Add NetworkValue", true);
                    menu.vanishIfMouseDistant = true;
                    Find.WindowStack.Add(menu);
                }
            };
        }
        */
    }
}