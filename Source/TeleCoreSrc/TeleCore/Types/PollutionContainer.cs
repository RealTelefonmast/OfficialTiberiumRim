namespace TeleCore.Types;

public class PollutionContainer
{
    public int ContainerCells { get; private set; }
    public int TotalCapacity { get; private set; }

    public float Saturation => Pollution / (float)TotalCapacity;
    public bool FullySaturated => Pollution >= TotalCapacity;

    public int Pollution { get; set; }

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