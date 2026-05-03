using System.Collections.Generic;
using TeleCore.Defs;
using Verse;

namespace TeleCore.Unsorted;

public class RealmConfig : Editable
{
    public AtmosphericRealm realmType;
    public List<AtmosphericValueDef> requiresAtmospheres;
}