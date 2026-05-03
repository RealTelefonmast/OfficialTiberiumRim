using UnityEngine;

namespace TiberiumRim;

public class CompTNW_Node : CompTNW
{
    public override Color[] ColorOverrides => new[] { Network.GeneralColor, Color.white };
}