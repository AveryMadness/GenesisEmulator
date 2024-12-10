namespace GenesisEmulator;

public class IoHandler : IMemoryHandler
{
    private Controller controller;

    public IoHandler(Controller cont)
    {
        controller = cont;
    }

    public byte ReadByte(uint address)
    {
        return controller.ReadPortByte(address);
    }

    public ushort ReadUInt16(uint address)
    {
        return controller.ReadPortUInt16(address);
    }

    public uint ReadUInt32(uint address)
    {
        return controller.ReadPortUInt32(address);
    }

    public void WriteByte(uint address, byte value)
    {
        controller.WritePortByte(address, value);
    }

    public void WriteUInt16(uint address, ushort value)
    {
        controller.WritePortUInt16(address, value);       
    }

    public void WriteUInt32(uint address, uint value)
    {
        controller.WritePortUInt32(address, value);
    }
}