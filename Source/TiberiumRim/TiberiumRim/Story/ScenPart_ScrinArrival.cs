using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class ScenPart_ScrinArrival : ScenPart
    {
        public override void GenerateIntoMap(Map map)
        {
            base.GenerateIntoMap(map);
        }

        public override void PostMapGenerate(Map map)
        {
            //Find.WindowStack.Add();

            base.PostMapGenerate(map);

        }

        public override void PostGameStart()
        {
            Find.MusicManagerPlay.disabled = true;
            Find.WindowStack.Notify_GameStartDialogOpened();
            DiaNode diaNode = new DiaNode("ScrinStartDialogue".Translate());
            DiaOption diaOption = new DiaOption();
            diaOption.resolveTree = true;
            diaOption.clickSound = null;
            diaNode.options.Add(diaOption);
            Dialog_NodeTree dialog_NodeTree = new Dialog_NodeTree(diaNode, false, false, null);
            dialog_NodeTree.soundClose = SoundDefOf.GameStartSting;
            dialog_NodeTree.closeAction = delegate ()
            {
                Find.MusicManagerPlay.ForceSilenceFor(7f);
                Find.MusicManagerPlay.disabled = false;
                Find.WindowStack.Notify_GameStartDialogClosed();
                Find.TickManager.CurTimeSpeed = TimeSpeed.Normal;
                TutorSystem.Notify_Event("GameStartDialogClosed");
            };
            Find.WindowStack.Add(dialog_NodeTree);
            Find.DesignatorManager.Select(new Designator_ScrinLanding());
        }
    }
}
