using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class ScrinWorkerDrone : MechanicalPawn
    {
        public override void PostMake()
        {
            base.PostMake();
            if (ownership == null)
            {
                ownership = new Pawn_Ownership(this);
            }
            if (skills == null)
            {
                skills = new Pawn_SkillTracker(this);
            }
            if (story == null)
            {
                story = new Pawn_StoryTracker(this);
            }
            if (guest == null)
            {
                guest = new Pawn_GuestTracker(this);
            }
            if (guilt == null)
            {
                guilt = new Pawn_GuiltTracker();
            }
            if (workSettings == null)
            {
                workSettings = new Pawn_WorkSettings(this);
                workSettings.EnableAndInitializeIfNotAlreadyInitialized();
                foreach (var workTypeDef in DefDatabase<WorkTypeDef>.AllDefs)
                {
                    workSettings.SetPriority(workTypeDef, 1);
                }
            }
        }
    }
}