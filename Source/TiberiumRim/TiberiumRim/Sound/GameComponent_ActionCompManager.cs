using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace TiberiumRim
{
    public class GameComponent_ActionCompManager : GameComponent
    {
        private List<ActionComposition> Compositions = new List<ActionComposition>();

        public GameComponent_ActionCompManager(Game game){}

        public void InitComposition(ActionComposition composition)
        {
            Compositions.Add(composition);
        }

        public void RemoveComposition(ActionComposition composition)
        {
            Compositions.Remove(composition);
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            for (int i = Compositions.Count - 1; i >= 0; i--)
            {
                Compositions[i].Tick();
            }
        }
    }
}

