using System.Collections.Generic;

namespace TeleCore.Unsorted;

public class TelePatch
{
    public virtual IEnumerable<string> RequiredAssemblyPath()
    {
        yield break;
    }
}