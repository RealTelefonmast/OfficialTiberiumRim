using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace TeleCore.Patching;

public class TelePatch
{
    public virtual IEnumerable<string> RequiredAssemblyPath()
    {
        yield break;
    }
}