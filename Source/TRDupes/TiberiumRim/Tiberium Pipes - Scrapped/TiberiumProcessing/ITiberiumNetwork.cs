using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public interface ITiberiumNetwork 
    {
        void HandlePipes(List<TNW_Pipe> pipes);
        void HandleConnections(List<TiberiumNetworkBuilding> tnwbs);


    }
}
