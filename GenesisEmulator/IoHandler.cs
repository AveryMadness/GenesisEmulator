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

    public ushort ReadUInt16(uint address)
    {
        return 0x0000;
    }

    public uint ReadUInt32(uint address)
    {
        return 0x000000;
    }

    public void WriteByte(uint address, byte value)
    {
        writeCallback(address, value);
    }

    public void WriteUInt16(uint address, ushort value)
    {
        //
    }

    public void WriteUInt32(uint address, uint value)
    {
        //
    }
}