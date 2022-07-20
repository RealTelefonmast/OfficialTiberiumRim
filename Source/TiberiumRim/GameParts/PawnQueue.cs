using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TiberiumRim
{
    public class PawnQueue
    {
        public List<Pawn> pawnList;

        public PawnQueue()
        {
            pawnList = new();
        }

        public void Enqueue(Pawn pawn)
        {
            pawnList.Add(pawn);
        }

        public Pawn Dequeue()
        {
            if (pawnList.Count <= 0) return null;
            var pawn = pawnList[0];
            pawnList.RemoveAt(0);
            return pawn;
        }

        public void Remove(Pawn pawn)
        {
            if (!pawnList.Remove(pawn))
            {
                TRLog.Warning($"Failed to remove {pawn.NameShortColored} from queue: {pawnList.ToStringSafeEnumerable()}");
            }
        }

        public bool TryPeek(out Pawn pawn)
        {
            pawn = null;
            if (pawnList.Count <= 0) return false;
            pawn = pawnList[0];
            return true;
        }

        public override string ToString()
        {
            return $"{pawnList.ToStringSafeEnumerable()}";
        }

        public bool Contains(Pawn pawn)
        {
            return pawnList.Contains(pawn);
        }
    }

}
