using System.Collections.Generic;
using Verse;

namespace TR.TiberiumEnvironment;

public class TerrainDataDef : Def
{
    public List<TerrainData> terrain;
}

/*  The Tiberium Flora Grid keeps track of all cells that are eligibale and meant for Tiberium plant life,
 *  This is used for a more organic look of the map once it gets covered with Tiberium
 */