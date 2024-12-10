namespace GenesisEmulator;

public interface IMemoryHandler
{
    byte ReadByte(uint address);
    UInt16 ReadUInt16(uint address);
    UInt32 ReadUInt32(uint address);
    void WriteByte(uint address, byte value);
    void WriteUInt16(uint address, UInt16 value);
    void WriteUInt32(uint address, UInt32 value);
}

public class MemoryManager
{
    private readonly Dictionary<(uint start, uint end), IMemoryHandler> memoryMap = new();

    // Add a memory region
    public void AddMemoryRegion(uint start, uint end, IMemoryHandler handler)
    {
        memoryMap[(start, end)] = handler;
    }

    // Find the appropriate handler for an address
    private IMemoryHandler GetHandler(uint address)
    {
        foreach (var (range, handler) in memoryMap)
        {
            if (address >= range.start && address <= range.end)
                return handler;
        }
        throw new Exception($"Unmapped memory access at address 0x{address:X8}");
    }

    public byte ReadByte(uint address)
    {
        return GetHandler(address).ReadByte(address);
    }

    public UInt16 ReadUInt16(uint address)
    {
        return GetHandler(address).ReadUInt16(address);
    }

    public UInt32 ReadUInt32(uint address)
    {
        return GetHandler(address).ReadUInt32(address);
    }

    public void WriteByte(uint address, byte value)
    {
        GetHandler(address).WriteByte(address, value);
    }

    public void WriteUInt16(uint address, UInt16 value)
    {
        GetHandler(address).WriteUInt16(address, value);
    }
    
    public void WriteUInt32(uint address, UInt32 value)
    {
        GetHandler(address).WriteUInt32(address, value);
    }
}

public class Memory
{
    
}