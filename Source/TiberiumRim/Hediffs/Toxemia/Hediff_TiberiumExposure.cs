namespace TiberiumRim
{
    public class Hediff_TiberiumExposure : Hediff_CauseToxemia
    {
        public override string LabelInBrackets => base.LabelInBrackets;

        private bool ResistDeath
        {
            get
            {
                foreach (var hediff in pawn.health.hediffSet.hediffs)
                {
                    if (hediff is Hediff_TiberiumMutation mut || (hediff is Hediff_TiberiumToxemia tox && tox.ToxemiaSeverity > 0.5))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public override bool CauseDeathNow()
        {
            return base.CauseDeathNow();
        }
    }
}
