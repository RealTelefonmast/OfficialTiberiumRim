using System;
using Verse.AI.Group;

namespace TiberiumRim
{
    public class LordJob_UseAirlock : LordJob
    {
        public override StateGraph CreateGraph()
        {
            throw new NotImplementedException();
        }

        public override bool KeepExistingWhileHasAnyBuilding => true;
    }
}
