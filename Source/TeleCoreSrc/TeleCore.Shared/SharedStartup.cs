using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TeleCore.Loader;
using Verse;

namespace TeleCore.Shared;

[TeleCoreStartupClass]
public static class SharedStartup
{
    static SharedStartup()
    {
        TeleCoreStaticStartup.OnStartup += OnStartup;
    }

    private static void OnStartup()
    {
        TLog.Message("Starting TeleCore.Shared...");
    }
}