namespace TiberiumRim
{
    public class PollutionContainer
    {
        private int pollutionInt = 0;

        public int ContainerCells { get; private set; } = 0;
        public int TotalCapacity { get; private set; } = 0;

        public float Saturation => Pollution / (float)TotalCapacity;
        public bool FullySaturated => Pollution >= TotalCapacity;

        public int Pollution
        {
            get => pollutionInt;
            set => pollutionInt = value;
        }

        public bool TryPollute(int value)
        {
            if (FullySaturated) return false;
            Pollution += value;
            return true;
        }

        //Set New Data When RoomComp changes (important with Map-Rooms)
        public void RegenerateData(int roomCells)
        {
            ContainerCells = roomCells;
            TotalCapacity = ContainerCells * TiberiumPollutionMapInfo.CELL_CAPACITY;
        }
    }
}
