using System;
using Verse.AI.Group;

namespace TR.AI;

public class LordJob_UseAirlock : LordJob
{
    public override bool KeepExistingWhileHasAnyBuilding => true;

    public override StateGraph CreateGraph()
    {
        throw new NotImplementedException();
    }
}