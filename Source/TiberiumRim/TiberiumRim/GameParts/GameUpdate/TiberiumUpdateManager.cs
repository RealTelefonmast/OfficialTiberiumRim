using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TiberiumRim
{
    /// <summary>
    /// Offloaded management of Tiberium-Related content
    /// </summary>
    public class TiberiumUpdateManager
    {
        public TickManager BaseTickManager => Find.TickManager;
        public bool GameRunning => Current.Game != null && !Find.TickManager.Paused;

        public void Update()
        {

        }
    }
}
