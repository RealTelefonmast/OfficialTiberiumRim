using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;

namespace TiberiumRim
{
    [StaticConstructorOnStartup]
    public class MainTabWindow_MissionObjectives : MainTabWindow
    {
        public WorldComponent_TiberiumMissions Missions;

        public Vector2 scrollPosLeft = Vector2.zero;

        public Vector2 scrollPosObj = Vector2.zero;

        public Mission selectedMission;

        public MissionObjectiveDef selectedObjective;

        private int imgNum = 0;

        private Dictionary<MissionObjectiveDef, Vector2> skillScroll = new Dictionary<MissionObjectiveDef, Vector2>();

        public List<Texture2D> imageRow = new List<Texture2D>();

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(1316f, 756f);
            }
        }

        public override void PreOpen()
        {
            base.PreOpen();
            this.Missions = Find.World.GetComponent<WorldComponent_TiberiumMissions>();
            this.selectedMission = Missions.Missions.FirstOrDefault();
        }

        public List<Mission> AvailableMissions
        {
            get
            {
                return Missions.Missions;
            }
        }

        public List<Pawn> CapablePawns(MissionObjectiveDef objective)
        {
            return Find.AnyPlayerHomeMap.mapPawns.AllPawns.Where(p => p.IsColonist && objective.skillRequirements.All((SkillRequirement x) => p.skills.skills.Any(sr2 => sr2.def == x.skillDef && sr2.levelInt >= x.skillLevel))).ToList();
        }

        public override void DoWindowContents(Rect inRect)
        {
            /*
            if ((this.selectedMission = this.Missions.Missions.FirstOrDefault((Mission x) => x.def.CanStartNow)) == null)
            {
                Text.Font = GameFont.Tiny;
                string s = "Thanks Mehni. Look what you made me do.";
                Vector2 v = Text.CalcSize(s);
                Rect rect = new Rect(new Vector2(0f, 0f), v);
                rect.center = inRect.center;
                Widgets.DrawMenuSection(rect);
                Widgets.Label(rect, s);
                return;
            }
            */
            Widgets.DrawTextureFitted(inRect, TRMats.WorkBanner, 1f);

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleLeft;

            float num = 40f;
            float yHeight = 600f;
            float yOffset = (inRect.height - yHeight) / 2f;
            Rect refRect = new Rect(0f, yOffset, inRect.width, yHeight);
            GUI.BeginGroup(refRect);
            float height = num * AvailableMissions.Count();

            Rect leftPart = new Rect(0f, 0f, 250f, refRect.height).ContractedBy(5f);

            Widgets.DrawMenuSection(leftPart);
            Rect viewRect = new Rect(0f, 0f, leftPart.width, height);
            GUI.BeginGroup(leftPart);
            Widgets.BeginScrollView(new Rect(0f, 0f, leftPart.width, leftPart.height), ref this.scrollPosLeft, viewRect, true);
            int num2 = 0;
            foreach (Mission mission in this.AvailableMissions)
            {
                Rect rect4 = new Rect(0f, (float)num2, leftPart.width, num);
                this.DoMissionTab(rect4, mission);
                num2 += (int)num;
            }
            Widgets.EndScrollView();
            GUI.EndGroup();
            Rect rightPart = new Rect(250f, 0f, refRect.width - 250, refRect.height).ContractedBy(5f);
            Widgets.DrawMenuSection(rightPart);
            rightPart = rightPart.ContractedBy(10f);
            GUI.BeginGroup(rightPart);
            if(selectedMission != null)
            {
                DoObjectiveWindow(rightPart, selectedMission);
            }
            GUI.EndGroup();
            GUI.EndGroup();
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private void DoMissionTab(Rect rect, Mission mission)
        {
            Missions.Notify_Seen(mission);

            rect = rect.ContractedBy(3f);
            if (Mouse.IsOver(rect) || this.selectedMission == mission)
            {
                GUI.color = Color.yellow;
                Widgets.DrawHighlight(rect);
            }
            GUI.color = Color.white;
            

            WidgetRow widgetRow = new WidgetRow(rect.x, rect.y + (rect.height - 24f) /2, UIDirection.RightThenUp, 99999f, 1f);
            
            if (mission != null && !mission.def.IsFinished)
            {
                widgetRow.Icon(ContentFinder<Texture2D>.Get("UI/Icons/objectiveActive", false), null);
            }
            else if (mission.failed)
            {
                widgetRow.Icon(ContentFinder<Texture2D>.Get("UI/Icons/objectiveFailed", false), null);
            }
            else if (mission.def.IsFinished)
            {
                widgetRow.Icon(ContentFinder<Texture2D>.Get("UI/Icons/objectiveFinished", false), null);
            }
            widgetRow.Gap(5f);
            widgetRow.Label(mission.def.label, 200f);
            if (Widgets.ButtonText(rect, "", false, true, Color.blue, true))
            {
                SoundDefOf.Click.PlayOneShotOnCamera(null);
                this.selectedMission = mission;
            }
        }

        private void DoObjectiveTab(Rect rect, MissionObjectiveDef obj, int curObj)
        {
            rect = rect.ContractedBy(1f);
            if (curObj % 2 == 0)
            {
                Widgets.DrawBoxSolid(rect, new ColorInt(50, 50, 50).ToColor);
            }
            GUI.BeginGroup(rect);
            WidgetRow widgetRow = new WidgetRow(5f, 0f, UIDirection.RightThenUp, 99999f, 4f);
            widgetRow.Label(obj.label, 200f);

            Rect CornerRect = new Rect(rect.xMax - 100f, 0f, 100f, 30f).ContractedBy(5f);           

            bool flag = obj.workCost > 0;
            float pct = 0;
            string label = "";

            if(obj.workCost > 0)
            {
                pct = obj.ProgressPct;
                label = Mathf.RoundToInt(obj.Progress) + "/" + obj.workCost;

                DoProgressBar(CornerRect, label, pct, TRMats.blue);
                CornerRect.y = 30f;
            }
            else if(obj.objectiveType != ObjectiveType.Wait)
            {
                pct = (obj.IsFinished ? 1f : 0f);
                label= obj.IsFinished ? "1/1" : "0/1";
                DoProgressBar(CornerRect, label, pct, TRMats.blue);
                CornerRect.y = 30f;
            }
            if (obj.TimerTicks > 0)
            {
                float timer = selectedMission.GetTimer(obj);
                pct = timer / obj.TimerTicks;
                if (timer > GenDate.TicksPerYear)
                {
                    label = Math.Round(timer / GenDate.TicksPerYear, 1) + "y";
                }
                else if (timer > GenDate.TicksPerDay)
                {
                    label = Math.Round(timer / GenDate.TicksPerDay, 1) + "d";
                }
                else if (timer < GenDate.TicksPerDay)
                {
                    label = Math.Round(timer / GenDate.TicksPerHour, 1) + "h";
                }
                DoProgressBar(CornerRect, label, pct, TRMats.grey);
            }

            Rect skillRect = new Rect();
            if (obj.skillRequirements.Count > 0)
            {
                foreach (MissionObjectiveDef objective in selectedMission.def.objectives)
                {
                    if (!skillScroll.Keys.Contains(objective))
                    {
                        skillScroll.Add(objective, new Vector2());
                    }
                }
                List<Pawn> pawnList = CapablePawns(obj);
                skillRect = new Rect(rect.xMax - (CornerRect.width * 2f) - 10f, 0f, CornerRect.width + 5f, rect.height).ContractedBy(5f);
                Widgets.DrawMenuSection(skillRect);
                
                Text.Anchor = TextAnchor.UpperCenter;
                Widgets.Label(skillRect, "Pawns: ");
                Text.Anchor = TextAnchor.LowerCenter;
                Widgets.Label(skillRect, "" + pawnList.Count);
                Text.Anchor = 0;

                StringBuilder skills = new StringBuilder();
                foreach (SkillRequirement skill in obj.skillRequirements)
                {
                    skills.AppendLine("    " + skill.skillDef.LabelCap + ": " + skill.skillLevel);
                }
                StringBuilder pawns = new StringBuilder();
                foreach(Pawn p in pawnList)
                {
                    pawns.AppendLine("    " + p.LabelCap);
                }
                TooltipHandler.TipRegion(skillRect, "ObjectiveSkills".Translate(new object[] {
                    skills,
                    pawnList.Count > 0 ? "ObjectivePawns".Translate(new object[]{
                        pawns
                    }) : ""
                }));
            }

            Text.Anchor = 0;
            if(!obj.stationDefs.NullOrEmpty())
            {
                string s = "StationNeeded".Translate() + ": " + obj.BestPotentialStationDef.LabelCap;
                Vector2 v2 = Text.CalcSize(s);
                Rect rect2 = new Rect(5f, rect.height / 2, v2.x, v2.y);
                TRUtils.DrawMenuSectionColor(rect2.ExpandedBy(1f), 1, new ColorInt(35,35,35), new ColorInt(85,85,85));
                Widgets.Label(rect2, s);

                if (obj.stationDefs.Count > 1)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (ThingDef def in obj.stationDefs)
                    {
                        sb.AppendLine("    " + def.LabelCap);
                    }
                    TooltipHandler.TipRegion(rect2, "PotentialStations".Translate(new object[] {
                    obj.BestPotentialStationDef.LabelCap,
                    sb
                }));
                }
            }

            GUI.EndGroup();
            if (obj.Failed)
            {
                GUI.color = Color.red;
                Widgets.DrawHighlight(rect);
                GUI.color = Color.white;
            }
            if (Mouse.IsOver(rect) || this.selectedObjective == obj)
            {
                GUI.color = Color.yellow;
                Widgets.DrawHighlight(rect);
                GUI.color = Color.white;
            }
            if (Widgets.ButtonText(new Rect(rect.x, rect.y, rect.width, rect.height), "", false, true, Color.blue, true))
            {
                if (Mouse.IsOver(skillRect))
                {
                    Find.Selector.SelectedObjects.Clear();
                    Find.Selector.Select(CapablePawns(obj)?.RandomElement());
                    this.Close();
                }
                SoundDefOf.Click.PlayOneShotOnCamera(null);
                this.selectedObjective = obj;
                imgNum = 0;
            }
        }

        public void DoProgressBar(Rect rect, string label, float pct, Texture2D barMat)
        {
            Widgets.FillableBar(rect, pct, barMat, TRMats.black, true);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect, label);
        }

        public void DoObjectiveWindow(Rect rect, Mission mission)
        {
            if (selectedObjective == null)
            {
                selectedObjective = mission.def.objectives.FirstOrDefault();
            }
            Text.Font = GameFont.Small;
            Rect descRect = new Rect(0, 0, rect.width/3, rect.height).ContractedBy(10f);
            Widgets.DrawMenuSection(descRect);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(selectedMission.def.description);
            sb.AppendLine("");
            sb.AppendLine(selectedObjective.description);

            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.Label(descRect.ContractedBy(5f), sb.ToString());
            Text.Anchor = 0;

            Rect imageRect = new Rect(rect.width / 3, 0, (rect.width * 2 )/3, rect.height/2).ContractedBy(10f);
            Rect imageSwapL = new Rect(imageRect.x, imageRect.y, 45f, imageRect.height).ContractedBy(5f);
            Rect imageSwapR = new Rect(imageRect.xMax - 45f, imageRect.y, 45f, imageRect.height).ContractedBy(5f);          

            Widgets.DrawShadowAround(imageRect);
            Widgets.DrawBoxSolid(imageRect, new Color(0.14f, 0.14f, 0.14f));

            if (selectedObjective != null)
            {
                SetDiaShow(selectedObjective);
                if (imageRow.Count > 0 && imgNum <= (imageRow.Count -1) && imageRow[imgNum] != null)
                {
                    Widgets.DrawTextureFitted(imageRect, imageRow[imgNum], 1f);
                }
            }
            if (Mouse.IsOver(imageSwapL))
            {
                if (imgNum > 0)
                {
                    GUI.color = Color.gray;
                    Widgets.DrawHighlight(imageSwapL);
                    if (Widgets.ButtonText(imageSwapL, "", false, true, Color.blue, true))
                    {
                        imgNum -= 1;
                    }
                }
            }
            if (Mouse.IsOver(imageSwapR))
            {
                if(imgNum < imageRow.Count -1)
                {
                    GUI.color = Color.gray;
                    Widgets.DrawHighlight(imageSwapR);
                    if (Widgets.ButtonText(imageSwapR, "", false, true, Color.blue, true))
                    {
                        imgNum += 1;
                    }
                }
            }

            float num = 60f;
            float height = num * mission.def.objectives.Count();
            Rect objectiveMenu = new Rect(rect.width / 3f, rect.height / 2f, rect.width - rect.width / 3, rect.height / 2).ContractedBy(10f);
            Widgets.DrawMenuSection(objectiveMenu);
            GUI.BeginGroup(objectiveMenu);
            Rect inRect = new Rect(0f, 0f, objectiveMenu.width, objectiveMenu.height).ContractedBy(1f);
            Rect viewRect = new Rect(0f, 0f, inRect.width, height);

            Widgets.BeginScrollView(inRect, ref this.scrollPosObj, viewRect, false);
            int num2 = 0;
            for(int i = 0; i < mission.def.objectives.Count; i++)
            {
                MissionObjectiveDef obj = mission.def.objectives[i];
                if (obj.Active)
                {
                    Rect rect4 = new Rect(0f, (float)num2, objectiveMenu.ContractedBy(1).width, num);
                    DoObjectiveTab(rect4, obj, i);
                    num2 += (int)num;
                }
            }
            Widgets.EndScrollView();
            GUI.EndGroup();
        }

        public void SetDiaShow(MissionObjectiveDef objective)
        {
            imageRow.Clear();
            foreach (string s in objective.images)
            {
                Texture2D text = ContentFinder<Texture2D>.Get(s, true);
                imageRow.Add(text);
            }
        }
    }
}
