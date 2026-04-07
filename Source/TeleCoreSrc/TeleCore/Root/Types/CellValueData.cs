namespace TeleCore.Types;

public struct CellValueData
{
    //Index of the cell
    public uint index;

    //Actual value
    public uint value;

    //CPU value input
    public uint inputValue;

    public CellValueData(uint index, uint value)
    {
        this.index = index;
        this.value = value;
        inputValue = 0;
    }
}