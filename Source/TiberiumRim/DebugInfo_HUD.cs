using System.Collections.Generic;

namespace TiberiumRim
{
    public static class DebugInfo_HUD
    {
        public static Dictionary<string, string> infos = new Dictionary<string, string>();

        public static void NewEntry(string ident, string val)
        {
            if (!infos.ContainsKey(ident))
            {
                infos.Add(ident, val);
                return;
            }
            infos[ident] = val;
        }
    }
}
