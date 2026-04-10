using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using RimWorld.Planet;
using TeleCore.Logging;
using TeleCore.Primitive.Immutable;
using Verse;

namespace TeleCore.Utils.Serialization.Scribe;

public class Scribe_Arrays
{
    public static void Look<T>(ref LightImmutableArray<T> immutArr, string label, LookMode lookMode = LookMode.Undefined,
        params object[] ctorArgs)
    {
        var arr = immutArr.ToArray();
        Look(ref arr, label, lookMode, ctorArgs);
        immutArr = new LightImmutableArray<T>(arr);
    }

    public static void Look<T>(ref ImmutableArray<T> immutArr, string label, LookMode lookMode = LookMode.Undefined,
        params object[] ctorArgs)
    {
        var arr = immutArr.ToArray();
        Look(ref arr, label, lookMode, ctorArgs);
        immutArr = ImmutableArray.Create(arr);
    }

    public static void Look<T>(ref T[] arr, string label, LookMode lookMode = LookMode.Undefined,
        params object[] ctorArgs)
    {
        Look(ref arr, false, label, lookMode, ctorArgs);
    }

    public static void Look<T>(ref T[] arr, bool saveDestroyedThings, string label,
        LookMode lookMode = LookMode.Undefined, params object[] ctorArgs)
    {
        if (lookMode == LookMode.Undefined && !Scribe_Universal.TryResolveLookMode(typeof(T), out lookMode))
        {
            TLog.Error("LookArray call with an array of " + typeof(T) + " must have lookMode set explicitly.");
            return;
        }

        if (Verse.Scribe.EnterNode(label))
            try
            {
                if (Verse.Scribe.mode == LoadSaveMode.Saving)
                {
                    if (arr == null)
                    {
                        Verse.Scribe.saver.WriteAttribute("IsNull", "True");
                        return;
                    }

                    for (var i = 0; i < arr.Length; i++)
                    {
                        var t = arr[i];
                        if (lookMode == LookMode.Value)
                        {
                            var t2 = t;
                            Scribe_Values.Look<T>(ref t2, "li", default, true);
                        }
                        else if (lookMode == LookMode.LocalTargetInfo)
                        {
                            var localTargetInfo = (LocalTargetInfo)(object)t;
                            Scribe_TargetInfo.Look(ref localTargetInfo, saveDestroyedThings, "li");
                        }
                        else if (lookMode == LookMode.TargetInfo)
                        {
                            var targetInfo = (TargetInfo)(object)t;
                            Scribe_TargetInfo.Look(ref targetInfo, saveDestroyedThings, "li");
                        }
                        else if (lookMode == LookMode.GlobalTargetInfo)
                        {
                            var globalTargetInfo = (GlobalTargetInfo)(object)t;
                            Scribe_TargetInfo.Look(ref globalTargetInfo, saveDestroyedThings, "li");
                        }
                        else if (lookMode == LookMode.Def)
                        {
                            var def = (Def)(object)t;
                            Scribe_Defs.Look(ref def, "li");
                        }
                        else if (lookMode == LookMode.BodyPart)
                        {
                            var bodyPartRecord = (BodyPartRecord)(object)t;
                            Scribe_BodyParts.Look(ref bodyPartRecord, "li");
                        }
                        else if (lookMode == LookMode.Deep)
                        {
                            var t3 = t;
                            Scribe_Deep.Look(ref t3, saveDestroyedThings, "li", ctorArgs);
                        }
                        else if (lookMode == LookMode.Reference)
                        {
                            var loadReferenceable = (ILoadReferenceable)t;
                            Scribe_References.Look(ref loadReferenceable, "li",
                                saveDestroyedThings);
                        }
                    }
                }

                if (Verse.Scribe.mode == LoadSaveMode.LoadingVars)
                {
                    var curXmlParent = Verse.Scribe.loader.curXmlParent;
                    var xmlAttribute = curXmlParent.Attributes["IsNull"];
                    if (xmlAttribute != null &&
                        xmlAttribute.Value.Equals("true", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (lookMode == LookMode.Reference)
                            Verse.Scribe.loader.crossRefs.loadIDs.RegisterLoadIDListReadFromXml(null, null);

                        arr = null;
                    }
                    else
                    {
                        if (lookMode == LookMode.Value)
                        {
                            arr = new T[curXmlParent.ChildNodes.Count];
                            for (var i = 0; i < curXmlParent.ChildNodes.Count; i++)
                            {
                                var obj = curXmlParent.ChildNodes[i];
                                var item = ScribeExtractor.ValueFromNode<T>((XmlNode)obj, default);
                                arr[i] = item;
                            }

                            return;
                        }

                        if (lookMode == LookMode.Deep)
                        {
                            arr = new T[curXmlParent.ChildNodes.Count];
                            for (var i = 0; i < curXmlParent.ChildNodes.Count; i++)
                            {
                                var obj = curXmlParent.ChildNodes[i];
                                var item = ScribeExtractor.SaveableFromNode<T>((XmlNode)obj, ctorArgs);
                                arr[i] = item;
                            }

                            return;
                        }

                        if (lookMode == LookMode.Def)
                        {
                            arr = new T[curXmlParent.ChildNodes.Count];
                            for (var i = 0; i < curXmlParent.ChildNodes.Count; i++)
                            {
                                var obj = curXmlParent.ChildNodes[i];
                                var item = ScribeExtractor.DefFromNodeUnsafe<T>((XmlNode)obj);
                                arr[i] = item;
                            }

                            return;
                        }

                        if (lookMode == LookMode.BodyPart)
                        {
                            arr = new T[curXmlParent.ChildNodes.Count];
                            for (var i = 0; i < curXmlParent.ChildNodes.Count; i++)
                            {
                                var obj = curXmlParent.ChildNodes[i];
                                var item = (T)(object)ScribeExtractor.BodyPartFromNode((XmlNode)obj, i.ToString(),
                                    null);
                                arr[i] = item;
                            }

                            return;
                        }

                        if (lookMode == LookMode.LocalTargetInfo)
                        {
                            arr = new T[curXmlParent.ChildNodes.Count];
                            for (var i = 0; i < curXmlParent.ChildNodes.Count; i++)
                            {
                                var obj = curXmlParent.ChildNodes[i];
                                var item = (T)(object)ScribeExtractor.LocalTargetInfoFromNode((XmlNode)obj,
                                    i.ToString(), LocalTargetInfo.Invalid);
                                arr[i] = item;
                            }

                            return;
                        }

                        if (lookMode == LookMode.TargetInfo)
                        {
                            arr = new T[curXmlParent.ChildNodes.Count];
                            for (var i = 0; i < curXmlParent.ChildNodes.Count; i++)
                            {
                                var obj = curXmlParent.ChildNodes[i];
                                var item = (T)(object)ScribeExtractor.TargetInfoFromNode((XmlNode)obj, i.ToString(),
                                    TargetInfo.Invalid);
                                arr[i] = item;
                            }

                            return;
                        }

                        if (lookMode == LookMode.GlobalTargetInfo)
                        {
                            arr = new T[curXmlParent.ChildNodes.Count];
                            for (var i = 0; i < curXmlParent.ChildNodes.Count; i++)
                            {
                                var obj = curXmlParent.ChildNodes[i];
                                var item = (T)(object)ScribeExtractor.GlobalTargetInfoFromNode((XmlNode)obj,
                                    i.ToString(), GlobalTargetInfo.Invalid);
                                arr[i] = item;
                            }

                            return;
                        }

                        if (lookMode == LookMode.Reference)
                        {
                            var list2 = new List<string>(curXmlParent.ChildNodes.Count);
                            foreach (var obj8 in curXmlParent.ChildNodes)
                            {
                                var xmlNode = (XmlNode)obj8;
                                list2.Add(xmlNode.InnerText);
                            }

                            Verse.Scribe.loader.crossRefs.loadIDs.RegisterLoadIDListReadFromXml(list2, "");
                        }
                    }
                }
                else if (Verse.Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
                {
                    if (lookMode == LookMode.Reference)
                    {
                        arr = Verse.Scribe.loader.crossRefs.TakeResolvedRefList<T>("").ToArray();
                    }
                    else if (lookMode == LookMode.LocalTargetInfo)
                    {
                        if (arr != null)
                            for (var i = 0; i < arr.Length; i++)
                                arr[i] = (T)(object)ScribeExtractor.ResolveLocalTargetInfo(
                                    (LocalTargetInfo)(object)arr[i], i.ToString());
                    }
                    else if (lookMode == LookMode.TargetInfo)
                    {
                        if (arr != null)
                            for (var j = 0; j < arr.Length; j++)
                                arr[j] = (T)(object)ScribeExtractor.ResolveTargetInfo((TargetInfo)(object)arr[j],
                                    j.ToString());
                    }
                    else if (lookMode == LookMode.GlobalTargetInfo && arr != null)
                    {
                        for (var k = 0; k < arr.Length; k++)
                            arr[k] = (T)(object)ScribeExtractor.ResolveGlobalTargetInfo(
                                (GlobalTargetInfo)(object)arr[k], k.ToString());
                    }
                }

                return;
            }
            finally
            {
                Verse.Scribe.ExitNode();
            }

        if (Verse.Scribe.mode == LoadSaveMode.LoadingVars)
        {
            if (lookMode == LookMode.Reference)
                Verse.Scribe.loader.crossRefs.loadIDs.RegisterLoadIDListReadFromXml(null, label);
            arr = null;
        }
    }
}