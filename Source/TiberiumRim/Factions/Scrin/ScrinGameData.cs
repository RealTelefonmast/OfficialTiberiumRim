namespace TR
{
    public class ScrinGameData
    {
        public int startingCredits = 10000;

        public int drones = 3;

        public int seeds = 1;

        public int CalculateCredits()
        {
            int creds = startingCredits;
            creds -= drones * 750;
            creds -= seeds * 2000;
            return creds;
        }
    }
}
