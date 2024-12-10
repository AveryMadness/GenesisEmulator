namespace GenesisEmulator;

public class IoHandler : IMemoryHandler
{
    private readonly Func<uint, byte> readCallback;
    private readonly Action<uint, byte> writeCallback;

    public IoHandler(Func<uint, byte> read, Action<uint, byte> write)
    {
        readCallback = read;
        writeCallback = write;
    }

    public byte ReadByte(uint address)
    {
        return readCallback(address);
    }

    public void WriteByte(uint address, byte value)
    {
        writeCallback(address, value);
    }
}