using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Multiplayer.API;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class Gizmo_NetworkInfo : Gizmo
    {
        private NetworkComponent parentComp;
        private string[] cachedStrings;

        private UILayout UILayout;
        private int mainWidth = 200;
        private int gizmoPadding = 5;

        private Dictionary<string, Action<Rect>> extensionSettings;

        public NetworkContainer Container => parentComp.Container;

        private bool HasExtension
        {
            get
            {
                //Requester overview
                if (parentComp.HasContainer) return true;
                if (parentComp.NetworkRole.HasFlag(NetworkRole.Requester)) return true;
                return false;
            }
        }

        public Gizmo_NetworkInfo(NetworkComponent parent) : base()
        {
            this.order = -250f;
            this.parentComp = parent;
            if (HasExtension)
            {
                TRFind.TickManager.RegisterTickAction(Tick);
                SetExtensions();
                SetUpExtensionUIData();
            }

            cachedStrings = new[]
            {
                $"{parentComp.NetworkDef}",
                $"{parentComp.NetworkRole}",
                $"Add NetworkValue",
            };
            UILayout = new UILayout();
            //
            var currentInspectTab = Find.MainTabsRoot.OpenTab.TabWindow as MainTabWindow_Inspect;
            Vector2 pos = new Vector2(0, currentInspectTab.PaneTopY);
            Vector2 size = currentInspectTab.RequestedTabSize;
            Vector2 titleSize = Text.CalcSize(cachedStrings[0]);

            Rect bgRect = new Rect(pos.x, pos.y, mainWidth + gizmoPadding, size.y);
            Rect settingsRect = new Rect(pos.x, pos.y - size.y, bgRect.width, bgRect.height);
            Rect mainRect = bgRect.ContractedBy(5f);
            Rect titleRect = new Rect(mainRect.x, mainRect.y, titleSize.x, titleSize.y);
            Vector2 roleTextSize = new Vector2(mainRect.width / 2, Text.CalcHeight(cachedStrings[1], mainRect.width / 2));
            Rect roleReadoutRect = new Rect(mainRect.x + roleTextSize.x, mainRect.y, roleTextSize.x, roleTextSize.y);
            Rect contentRect = new Rect(mainRect.x, titleRect.yMax, mainRect.width, mainRect.height - titleRect.height);
            Rect containerBarRect = new Rect(contentRect.x, contentRect.yMax - 16, contentRect.width / 2, 16);
            var padding = 5;
            var iconSize = 30;
            var width = iconSize + 2 * padding;
            var height = 2 * width;
            Rect buildOptionsRect = new Rect(contentRect.xMax - width, contentRect.yMax - height, width, height);

            UILayout.Register("BGRect", bgRect); //
            UILayout.Register("SettingsRect", settingsRect); //
            UILayout.Register("MainRect", mainRect); //
            UILayout.Register("TitleRect", titleRect); //
            UILayout.Register("RoleReadoutRect", roleReadoutRect);
            UILayout.Register("ContentRect", contentRect); //
            UILayout.Register("ContainerRect", containerBarRect); //
            UILayout.Register("BuildOptionsRect", buildOptionsRect); //
            UILayout.Register("ControllerOptionRect", buildOptionsRect.ContractedBy(5).TopPartPixels(iconSize)); //
            UILayout.Register("PipeOptionRect", buildOptionsRect.ContractedBy(5).BottomPartPixels(iconSize)); //
        }

        private string selectedSetting = null;
        private FloatRange extensionSettingYRange;
        private float desiredY;
        private float currentExtendedY = 0;

        private void Notify_ExtendHovered(bool isHovered)
        {
            desiredY = isHovered ? extensionSettingYRange.TrueMax : extensionSettingYRange.TrueMin;
        }

        private void Tick()
        {
            if (!Visible) return;
            if (Math.Abs(currentExtendedY - desiredY) > 0.01)
            {
                var val = desiredY > currentExtendedY ? 1.5f : -1.5f;
                currentExtendedY = Mathf.Clamp(currentExtendedY + val * extensionSettings.Count, extensionSettingYRange.TrueMin, extensionSettingYRange.TrueMax);
            }
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            UILayout.SetOrigin(new Vector2(topLeft.x - 15, 0));
            Rect rect = UILayout["BGRect"];

            //Extension Button
            if (HasExtension)
            {
                var mainRect = UILayout["MainRect"];
                var yMax = Math.Max(15, currentExtendedY) + 10;
                Rect extendTriggerArea = new Rect(mainRect.x, mainRect.y - (yMax - 5), mainRect.width, yMax);
                Rect extendedButton = new Rect(mainRect.x, mainRect.y - (currentExtendedY + 1), mainRect.width, currentExtendedY + 1);
                Notify_ExtendHovered(Mouse.IsOver(extendTriggerArea));

                Widgets.DrawWindowBackground(extendedButton);
                Text.Anchor = TextAnchor.MiddleCenter;
                var curY = extendedButton.y;
                foreach (var setting in extensionSettings)
                {
                    Rect labelRect = new Rect(extendedButton.x, curY, extendedButton.width, Math.Min(extendedButton.height, 20));
                    Widgets.Label(labelRect, setting.Key);
                    Widgets.DrawHighlightIfMouseover(labelRect);
                    if (Widgets.ButtonInvisible(labelRect))
                    {
                        selectedSetting = setting.Key;
                    }
                    curY += 20;
                }
                Text.Anchor = default;

                if (selectedSetting != null)
                {
                    var settingRect = UILayout["SettingsRect"];
                    var mouseOver = Mouse.IsOver(settingRect);
                    extensionSettings[selectedSetting].Invoke(settingRect);
                    if (Input.GetMouseButton(1) && mouseOver)
                    {
                        selectedSetting = null;
                    }
                }
            }

            Widgets.DrawWindowBackground(rect);
            Widgets.Label(UILayout["TitleRect"], cachedStrings[0]);
            Text.Anchor = TextAnchor.UpperRight;
            Widgets.Label(UILayout["RoleReadoutRect"], cachedStrings[1]);
            Text.Anchor = default;

            if (parentComp.HasContainer)
            {
                Rect containerRect = UILayout["ContainerRect"];
                Rect BarRect = containerRect.ContractedBy(2f);
                float xPos = BarRect.x;
                Widgets.DrawBoxSolid(containerRect, TRColor.Black);
                Widgets.DrawBoxSolid(BarRect, TRColor.White025);
                foreach (NetworkValueDef type in Container.AllStoredTypes)
                {
                    float percent = (Container.ValueForType(type) / Container.Capacity);
                    Rect typeRect = new Rect(xPos, BarRect.y, BarRect.width * percent, BarRect.height);
                    Color color = type.valueColor;
                    xPos += BarRect.width * percent;
                    Widgets.DrawBoxSolid(typeRect, color);
                }

                //Draw Hovered Readout
                if (!Container.Empty && Mouse.IsOver(containerRect))
                {
                    var mousePos = Event.current.mousePosition;
                    var containerReadoutSize = TRWidgets.GetTiberiumReadoutSize(Container);
                    Rect rectAtMouse = new Rect(mousePos.x, mousePos.y - containerReadoutSize.y, containerReadoutSize.x,
                        containerReadoutSize.y);
                    Widgets.DrawMenuSection(rectAtMouse);
                    TRWidgets.DrawTiberiumReadout(rectAtMouse, Container);
                }
            }

            //Custom Behaviour
            switch (parentComp.NetworkRole)
            {
                case NetworkRole.Requester:
                {

                }
                    break;
            }

            //Do network build options
            TRWidgets.DrawBoxHighlight(UILayout["BuildOptionsRect"]);
            var controllDesignator = StaticData.GetDesignatorFor<Designator_Build>(parentComp.NetworkDef.controllerDef);
            var pipeDesignator = StaticData.GetDesignatorFor<Designator_Build>(parentComp.NetworkDef.transmitter);
            if (Widgets.ButtonImage(UILayout["ControllerOptionRect"], controllDesignator.icon))
            {
                controllDesignator.ProcessInput(Event.current);
            }

            if (Widgets.ButtonImage(UILayout["PipeOptionRect"], pipeDesignator.icon))
            {
                pipeDesignator.ProcessInput(Event.current);
            }

            //
            TRWidgets.AbsorbInput(rect);
            return new GizmoResult(GizmoState.Mouseover);
        }



        public override float GetWidth(float maxWidth)
        {
            return mainWidth;
        }

        private void SetUpExtensionUIData()
        {
            extensionSettingYRange = new FloatRange(10, 20 * extensionSettings.Count);

        }

        private void SetExtensions()
        {
            extensionSettings = new Dictionary<string, Action<Rect>>();
            if (parentComp.NetworkRole.HasFlag(NetworkRole.Requester))
            {
                extensionSettings.Add("Requester Settings", delegate(Rect rect)
                {
                    Widgets.DrawWindowBackground(rect);

                    var contentRect = rect.ContractedBy(5);
                    GUI.BeginGroup(contentRect);
                    contentRect = contentRect.AtZero();

                    var curX = 0;
                    var allowedTypes = Container.AcceptedTypes;
                    foreach (var type in allowedTypes)
                    {
                        Rect typeRect = new Rect(curX, contentRect.height - 10, 10, 10);
                        Rect typeSliderSetting = new Rect(curX, contentRect.height - (15 + 100), 10, 100);
                        Widgets.DrawBoxSolid(typeRect, type.valueColor);
                        var previousValue = parentComp.RequestedTypes[type];
                        var newValue = GUI.VerticalSlider(typeSliderSetting, previousValue, Container.Capacity, 0);
                        parentComp.RequestedTypes[type] = newValue;
                        var totalRequested = parentComp.RequestedTypes.Values.Sum();
                        if (totalRequested > Container.Capacity)
                        {
                            if (previousValue < newValue)
                            {
                                var diff = newValue - previousValue;
                                foreach (var type2 in allowedTypes)
                                {
                                    if(type2 == type) continue;
                                    parentComp.RequestedTypes[type2] = Mathf.Lerp(0, parentComp.RequestedTypes[type2], 1f - Mathf.InverseLerp(0, Container.Capacity, newValue));
                                    //parentComp.RequestedTypes[type2] = Mathf.Clamp(parentComp.RequestedTypes[type2] - (diff / (parentComp.RequestedTypes.Count - 1)), 0, Container.Capacity);
                                }
                            }
                        }
                        curX += 20 + 5;
                    }

                    GUI.EndGroup();

                    //
                    TRWidgets.AbsorbInput(rect);
                });
            }

            if (parentComp.HasContainer)
            {
                extensionSettings.Add("Container Settings", delegate(Rect rect)
                {
                    Widgets.DrawWindowBackground(rect);
                    TRWidgets.DrawTiberiumReadout(rect, parentComp.Container);

                    //Right Click Input
                    if (TRWidgets.MouseClickIn(rect, 1))
                    {
                        FloatMenu menu = new FloatMenu(RightClickFloatMenuOptions.ToList(), cachedStrings[2], true);
                        menu.vanishIfMouseDistant = true;
                        Find.WindowStack.Add(menu);
                    }

                    //
                    TRWidgets.AbsorbInput(rect);
                });
            }
        }

        [SyncMethod]
        private void Debug_AddAll(float part)
        {
            foreach (var type in Container.AcceptedTypes)
            {
                Container.TryAddValue(type, part, out _);
            }
        }

        [SyncMethod]
        private void Debug_Clear()
        {
            Container.Clear();
        }

        [SyncMethod]
        private void Debug_AddType(NetworkValueDef type, float part)
        {
            Container.TryAddValue(type, part, out _);
        }

        public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
        {
            get
            {
                float part = Container.Capacity / Container.AcceptedTypes.Count;
                yield return new FloatMenuOption("Add ALL", delegate { Debug_AddAll(part); });

                yield return new FloatMenuOption("Remove ALL", Debug_Clear);

                foreach (var type in Container.AcceptedTypes)
                {
                    yield return new FloatMenuOption($"Add {type}", delegate
                    {
                        Debug_AddType(type, part);
                    });
                }
            }
        }
    }
}
