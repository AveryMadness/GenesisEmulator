namespace GenesisEmulator;

public class Ram : IMemoryHandler
{
    private readonly byte[] ram;

    public Ram(int size)
    {
        ram = new byte[size];
    }

    public byte ReadByte(uint address)
    {
        Console.WriteLine($"Cpu reading byte from address 0x{address:X8}");
        return ram[address % (uint)ram.Length];
    }

    public UInt16 ReadUInt16(uint address)
    {
        Console.WriteLine($"Cpu reading ushort from address 0x{address:X8}");
        return (ushort)((ReadByte(address) << 8) | ReadByte(address + 1));
    }

    public UInt32 ReadUInt32(uint address)
    {
        Console.WriteLine($"Cpu reading uint from address 0x{address:X8}");
        return (uint)((ReadByte(address) << 24) | (ReadByte(address + 1) << 16) | (ReadByte(address + 2) << 8) |
                      ReadByte(address + 3));
    }

    public void WriteByte(uint address, byte value)
    {
        Console.WriteLine($"Cpu writing byte to address 0x{address:X8}, value 0x{value:X8}");
        ram[address % (uint)ram.Length] = value;
    }

    public void WriteUInt16(uint address, UInt16 value)
    {
        Console.WriteLine($"Cpu writing ushort to address 0x{address:X8}, value 0x{value:X8}");
        WriteByte(address, (byte)(value >> 8));
        WriteByte(address + 1, (byte)(value & 0xFF));
    }

    public void WriteUInt32(uint address, UInt32 value)
    {
        Console.WriteLine($"Cpu writing uint to address 0x{address:X8}, value 0x{value:X8}");
        WriteByte(address, (byte)(value >> 24));
        WriteByte(address + 1, (byte)((value >> 16) & 0xFF));
        WriteByte(address + 2, (byte)((value >> 8) & 0xFF));
        WriteByte(address + 3, (byte)(value & 0xFF));
    }
}