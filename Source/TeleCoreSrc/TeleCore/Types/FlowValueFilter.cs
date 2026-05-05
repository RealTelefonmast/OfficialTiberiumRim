using System.Collections.Generic;
using TeleCore.Defs;

namespace TeleCore.Types;

public class FlowValueFilter<TValue>
    where TValue : FlowValueDef
{
    public List<TValue> allowedValues;
}