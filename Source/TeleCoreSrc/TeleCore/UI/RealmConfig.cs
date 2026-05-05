using System.Collections.Generic;
using TeleCore.Defs;
using Verse;
using AtmosphericRealm = TeleCore.Types.Enums.AtmosphericRealm;

namespace TeleCore.UI;

public class RealmConfig : Editable
{
    public AtmosphericRealm realmType;
    public List<AtmosphericValueDef> requiresAtmospheres;
}