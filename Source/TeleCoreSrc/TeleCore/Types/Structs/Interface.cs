namespace TeleCore.Types.Structs;

public struct Interface
{
    private Adapter[] array;

    public Interface(int length)
    {
        array = new Adapter[length];
    }
}