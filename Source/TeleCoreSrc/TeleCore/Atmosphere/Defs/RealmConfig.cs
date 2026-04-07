using System.Collections.Generic;
using Verse;

namespace TeleCore.Atmosphere.Defs;

public class RealmConfig : Editable
{
    public AtmosphericRealm realmType;
    public List<AtmosphericValueDef> requiresAtmospheres;
}