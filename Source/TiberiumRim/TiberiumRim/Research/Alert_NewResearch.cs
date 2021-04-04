using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;

namespace TiberiumRim
{
    public class Alert_NewResearch : Alert
    {
        public override AlertReport GetReport()
        {
            return false;
        }
    }
}
