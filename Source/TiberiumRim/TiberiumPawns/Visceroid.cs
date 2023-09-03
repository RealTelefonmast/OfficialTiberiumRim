using System.Collections.Generic;

namespace TR
{
    public class Visceroid : Pawn_Visceral
    {
        public bool TryFindOther(out List<Visceroid> list)
        {
            list = new List<Visceroid>();
            return false;
        }
    }
}
