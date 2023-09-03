using RimWorld;
using Verse;

namespace TR
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
                foreach (var skill in skills.skills)
                {
                    skill.levelInt = 20;
                    skill.passion = Passion.Major;
                }
                
            }
            if (story == null)
            {
                story = new Pawn_StoryTracker(this);
                story.title = "yes";
                story.traits = new TraitSet(this);
                
            }
            if (guest == null)
            {
                guest = new Pawn_GuestTracker(this);
            }
            if (guilt == null)
            {
                guilt = new Pawn_GuiltTracker(this);
            }
            if (workSettings == null)
            {
                workSettings = new Pawn_WorkSettings(this);
                workSettings.EnableAndInitializeIfNotAlreadyInitialized();
                foreach (var workTypeDef in DefDatabase<WorkTypeDef>.AllDefs)
                {
                    workSettings.SetPriority(workTypeDef, 1);
                }
                workSettings.Notify_UseWorkPrioritiesChanged();
            }

            Name = new NameSingle("Drone");
        }

        
    }
}