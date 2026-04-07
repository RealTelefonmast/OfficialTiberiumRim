using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using TeleCore.Lib.Utils;
using TeleCore.Loader;
using MonoMod.Utils;
using Verse;

namespace TeleCore.RWLib;

public class ScribeDelegate<TDelegate> : IExposable where TDelegate : Delegate
{
    //Temp Scribe Data
    private static byte[] _TempBytes;
    public TDelegate? @delegate;

    private MethodInfo? loadedMethod;
    private List<LookMode>? lookModes;
    private List<Type>? types;

    private object[]? universal;

    public ScribeDelegate()
    {
    }

    public ScribeDelegate(TDelegate? action)
    {
        @delegate = action;
    }

    public object Target => @delegate.Target;

    public void ExposeData()
    {
        var isSaving = Scribe.mode == LoadSaveMode.Saving;
        var isLoading = Scribe.mode == LoadSaveMode.LoadingVars;
        var isCrossRef = Scribe.mode == LoadSaveMode.ResolvingCrossRefs;
        var isPostLoad = Scribe.mode == LoadSaveMode.PostLoadInit;

        var scribeTimeMethod = loadedMethod;

        //## LoadSaving Method
        if (isSaving)
        {
            //Save delegate MethodInfo into serialized bytes
            _TempBytes = MethodConstructor.Serialize(@delegate);
            DataExposeUtility.LookByteArray(ref _TempBytes, "delegateBytes");

            //
            scribeTimeMethod = @delegate.Method;
        }

        if (isLoading)
        {
            //
            DataExposeUtility.LookByteArray(ref _TempBytes, "delegateBytes");
            scribeTimeMethod = loadedMethod = MethodConstructor.Deserialize(_TempBytes);
        }

        //## Load Method Target
        var usedDeclaringType = scribeTimeMethod.DeclaringType;
        var declaringTypeFields = usedDeclaringType.GetFields();

        //If anonymous class - save internal fields
        var isAnonymousType = usedDeclaringType.IsAnonymousType(out var isDisplayClass);
        if (isAnonymousType && isDisplayClass)
        {
            if (isSaving)
            {
                //Save LookModes for internal 
                var typesLooks = LookModes(@delegate.Target, declaringTypeFields);
                lookModes = typesLooks.Item2;
                types = typesLooks.Item1;

                //
                for (var i = 0; i < declaringTypeFields.Length; i++)
                {
                    var field = declaringTypeFields[i];
                    var unType = types[i];

                    var val = field.GetValue(@delegate.Target);
                    TryScribe(ref val, ref unType, field, lookModes[i]);
                }
            }

            if (isSaving || isLoading)
            {
                Scribe_Collections.Look(ref lookModes, "lookModes", LookMode.Value);
                Scribe_Collections.Look(ref types, "types", LookMode.Value);
            }

            if (isLoading)
            {
                universal = new object[declaringTypeFields.Length];

                //
                for (var i = 0; i < declaringTypeFields.Length; i++)
                {
                    var field = declaringTypeFields[i];
                    var unType = types[i];

                    object val = null;
                    TryScribe(ref val, ref unType, field, lookModes[i]);
                    universal[i] = val;
                }
            }

            if (isCrossRef)
                for (var i = 0; i < declaringTypeFields.Length; i++)
                {
                    var unType = types[i];
                    var val = universal[i];

                    TryScribe(ref val, ref unType, declaringTypeFields[i], lookModes[i]);
                    universal[i] = val;
                }

            if (isPostLoad)
            {
                var methodTarget = Activator.CreateInstance(usedDeclaringType);
                for (var s = 0; s < declaringTypeFields.Length; s++)
                {
                    var field = declaringTypeFields[s];
                    field.SetValue(methodTarget, universal[s]);
                }

                @delegate = scribeTimeMethod.CreateDelegate(typeof(TDelegate), methodTarget) as TDelegate;
            }
        }
        else
        {
            ILoadReferenceable loadReferencable = null;
            if (isSaving || isLoading || isCrossRef)
            {
                if (isSaving) loadReferencable = @delegate.Target as ILoadReferenceable;

                if (loadReferencable != null) Scribe_References.Look(ref loadReferencable, "referencable");
            }

            if (isPostLoad)
            {
                if (loadReferencable == null)
                {
                    if (isAnonymousType)
                        @delegate = (TDelegate)scribeTimeMethod.CreateDelegate(typeof(TDelegate), null);
                    else
                        TLog.Error(
                            $"Could not load a reference of type {usedDeclaringType} for method {scribeTimeMethod.Name}. Add an {nameof(ILoadReferenceable)} interface.");
                }
                else
                {
                    @delegate = (TDelegate)scribeTimeMethod.CreateDelegate(typeof(TDelegate), loadReferencable);
                }
            }
        }

        TLog.Debug($"Loaded Action: {@delegate?.Method.Name} in {@delegate?.Target}");
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            universal = null;
            lookModes = null;
            types = null;
            loadedMethod = null;
            _TempBytes = null;
        }
    }

    public static explicit operator TDelegate?(ScribeDelegate<TDelegate> e)
    {
        return e.@delegate;
    }

    private (List<Type>, List<LookMode>?) LookModes(object target, FieldInfo[] infos)
    {
        var looks = new LookMode[infos.Length];
        var types = new Type[infos.Length];
        for (var l = 0; l < infos.Length; l++)
        {
            var val = infos[l].GetValue(target);
            types[l] = val.GetType();
            if (val is ILoadReferenceable lr) looks[l] = LookMode.Reference;
            if (val is Def) looks[l] = LookMode.Def;
            if (types[l].IsValueType) looks[l] = LookMode.Value;
        }

        return (types.ToList(), looks.ToList());
    }

    private void TryScribe(ref object val, ref Type valType, FieldInfo field, LookMode mode)
    {
        TLog.Message($"Trying to scribe {val} of {valType} in {field} with {mode}");
        switch (mode)
        {
            case LookMode.Value:
                Scribe_Values.Look(ref val, field.Name);
                break;
            case LookMode.Reference:
                ILoadReferenceable tempRef = null;
                if (Scribe.mode == LoadSaveMode.Saving)
                    tempRef = (ILoadReferenceable)val;
                Scribe_References.Look(ref tempRef, field.Name);
                val = tempRef;
                break;
            case LookMode.Def:
                Def valDef = null;
                if (Scribe.mode == LoadSaveMode.Saving)
                    valDef = (Def)val;

                Scribe_Defs.Look(ref valDef, field.Name);
                val = valDef;
                break;
            default:
                Scribe_Universal.Look(ref val, field.Name, mode, ref valType);
                break;
        }
    }
}

internal sealed class MethodConstructor
{
    public static byte[] Serialize(Delegate? d)
    {
        return Serialize(d.Method);
    }

    public static byte[] Serialize(MethodInfo method)
    {
        using var stream = new MemoryStream();
        new BinaryFormatter().Serialize(stream, method);
        stream.Seek(0, SeekOrigin.Begin);
        return stream.ToArray();
    }

    public static MethodInfo? Deserialize(byte[] data)
    {
        using var stream = new MemoryStream(data);
        var method = (MethodInfo)new BinaryFormatter().Deserialize(stream);
        return method;
    }
}