namespace GenesisEmulator;

public class RomHandler : IMemoryHandler
{
    private readonly byte[] rom;

    public RomHandler(byte[] romData)
    {
        rom = romData;
    }

    public byte ReadByte(uint address)
    {
        Console.WriteLine($"Cpu reading byte from rom at 0x{address:X8}");
        if (address < rom.Length)
            return rom[address];
        return 0xFF; // Open bus
    }

    public UInt16 ReadUInt16(uint address)
    {
        Console.WriteLine($"Cpu reading ushort from rom at 0x{address:X8}");
        if (address < rom.Length)
        {
            byte highByte = rom[address];
            byte lowByte = rom[address + 1];
            return (ushort)((highByte << 8) | lowByte);
        }

        return 0xFFFF;
    }

    public UInt32 ReadUInt32(uint address)
    {
        Console.WriteLine($"Cpu reading uint from rom at 0x{address:X8}");

        if (address < rom.Length)
        {
            byte byte11 = rom[address];
            byte byte21 = rom[address + 1];
            byte byte31 = rom[address + 2];
            byte byte41 = rom[address + 3];
            return (uint)((byte11 << 24) | (byte21 << 16) | (byte31 << 8) | byte41);
        }

        return 0xFFFFFFFF;
    }

    public void WriteByte(uint address, byte value)
    {
        // Ignore writes to ROM
    }

    public void WriteUInt16(uint address, ushort value)
    {
        // no
    }

    public void WriteUInt32(uint address, uint value)
    {
        // still no
    }
}