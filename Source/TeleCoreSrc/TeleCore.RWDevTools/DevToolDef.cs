using System;
using Verse;

namespace TeleCore.RWDevTools;

public class DevToolDef : Def
{
    public Type windowClass;
    private Window windowInt;

    public Window GetWindow => windowInt ??= (Window)Activator.CreateInstance(windowClass);
}