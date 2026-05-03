using System.Collections.Generic;
using TeleCore.Defs;

namespace TeleCore.Unsorted;

public class FlowValueFilter<TValue>
    where TValue : FlowValueDef
{
    public List<TValue> allowedValues;
}