using System.Collections.Generic;

namespace TeleCore.Types.Patches;

public class TelePatch
{
    public virtual IEnumerable<string> RequiredAssemblyPath()
    {
        yield break;
    }
}