using System.Collections.Generic;

namespace TiberiumRim;

public interface ITiberiumNetwork
{
    void HandlePipes(List<TNW_Pipe> pipes);
    void HandleConnections(List<TiberiumNetworkBuilding> tnwbs);
}