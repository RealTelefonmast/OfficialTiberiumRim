using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TiberiumRim
{
    public interface IMapWatcher
    {
        public bool IsSpyingNow { get; }
        public Map MapTarget { get; }
    }
}
