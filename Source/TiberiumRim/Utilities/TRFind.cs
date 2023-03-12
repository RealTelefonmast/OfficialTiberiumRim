namespace TiberiumRim
{
    public static class TRFind
    {
        private static TiberiumRoot rootInt;

        public static TiberiumRoot TRoot
        {
            get => rootInt;
            set => rootInt = value;
        }

        public static TiberiumTickManager TickManager
        {
            get => TRoot.TickManager;
        }
    }
}
