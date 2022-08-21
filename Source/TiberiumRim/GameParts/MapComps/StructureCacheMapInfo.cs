using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleCore;
using Verse;

namespace TiberiumRim
{
    public class StructureCacheMapInfo : MapInformation
    {
        public Dictionary<TRGroupDef, List<Thing>> CachedThingsByGroup = new ();
        public Dictionary<TRGroupDef, List<ThingWithComps>> CachedCompParentsByGroup = new ();
        public Dictionary<TRGroupDef, List<ThingComp>> CachedCompsByGroup = new ();

        public StructureCacheMapInfo(Map map) : base(map)
        {
        }

        public List<Thing> GetThingsFromGroup(TRGroupDef group)
        {
            return CachedThingsByGroup.TryGetValue(group);
        }

        public List<ThingWithComps> GetCompParentsFromGroup(TRGroupDef group)
        {
            if (group == null) return null;
            return CachedCompParentsByGroup.TryGetValue(group);
        }

        public List<ThingComp> GetCompsFromGroup(TRGroupDef group)
        {
            return CachedCompsByGroup.TryGetValue(group);
        }

        public void RegisterPart(TRGroupDef groupDef, object obj)
        {
            if (groupDef == null) return;
            switch (obj)
            {
                case Thing thing:
                {
                    if (!CachedThingsByGroup.ContainsKey(groupDef))
                    {
                        CachedThingsByGroup.Add(groupDef, new List<Thing>());
                    }
                    CachedThingsByGroup[groupDef].Add(thing);
                    
                    break;
                }
                case ThingComp comp:
                {
                    if (!CachedCompsByGroup.ContainsKey(groupDef))
                    {
                        CachedCompsByGroup.Add(groupDef, new List<ThingComp>());
                    }
                    CachedCompsByGroup[groupDef].Add(comp);

                    //Cache Parent
                    if (!CachedCompParentsByGroup.ContainsKey(groupDef))
                    {
                        CachedCompParentsByGroup.Add(groupDef, new List<ThingWithComps>());
                    }
                    CachedCompParentsByGroup[groupDef].Add(comp.parent);
                    break;
                }
            }
            if(groupDef.ParentGroup != null)
                RegisterPart(groupDef.ParentGroup, obj);
        }

        public void DeregisterPart(TRGroupDef groupDef, object obj)
        {
            if (groupDef == null) return;
            switch (obj)
            {
                case Thing thing:
                {
                    CachedThingsByGroup[groupDef].Remove(thing);
                    break;
                }
                case ThingComp comp:
                {
                    CachedCompsByGroup[groupDef].Remove(comp);
                    CachedCompParentsByGroup[groupDef].Remove(comp.parent);
                    break;
                }
            }
            if (groupDef.ParentGroup != null)
                DeregisterPart(groupDef.ParentGroup, obj);
        }
    }
}
