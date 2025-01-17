using System.Data;

namespace GenesisEmulator;

public class Controller
{
    private byte port1State = 0x00;
    private byte port2State = 0x00;

    private bool AButtonPressed = false;
    private bool BButtonPressed = false;
    private bool CButtonPressed = false;
    private bool StartPressed = false;
    private bool UpPressed = false;
    private bool DownPressed = false;
    private bool LeftPressed = false;
    private bool RightPressed = false;
    
    public uint GetControllerBitmask(bool a, bool b, bool c, bool start, bool up, bool down, bool left, bool right)
    {
        uint bitmask = 0xFFFFFFFF; // Start with all buttons unpressed (0xFF means all bits set to 1)
    
        // Set the bits according to button presses
        if (a) bitmask &= 0xFFFFFFFE; // A button
        if (b) bitmask &= 0xFFFFFFFD; // B button
        if (c) bitmask &= 0xFFFFFFFB; // C button
        if (start) bitmask &= 0xFFFFFFF7; // Start button
        if (up) bitmask &= 0xFFFFFFEF; // Up button
        if (down) bitmask &= 0xFFFFFFDF; // Down button
        if (left) bitmask &= 0xFFFFFFBF; // Left button
        if (right) bitmask &= 0xFFFFFF7F; // Right button
    
        return bitmask;
    }

    public void Start()
    {
        StartPressed = true;
    }

    public byte ReadPortByte(uint address)
    {
        Console.WriteLine($"Cpu reading byte from i/o address 0x{address:X8}");
        
        return address switch
        {
            0xA10003 => port1State,
            0xA10005 => port2State,
            _ => 0xFF
        };
    }

    public ushort ReadPortUInt16(uint address)
    {
        Console.WriteLine($"Cpu reading from i/o address 0x{address:X8}");

        return address switch
        {
            _ => 0xFFFF
        };
    }

    public uint ReadPortUInt32(uint address)
    {
        Console.WriteLine($"Cpu reading from i/o address 0x{address:X8}");

        return address switch
        {
            0xA10008 => GetControllerBitmask(AButtonPressed, BButtonPressed, CButtonPressed, StartPressed,
                UpPressed, DownPressed, LeftPressed, RightPressed),
            _ => 0xFFFFFFFF
        };
    }

    public void WritePortByte(uint address, byte value)
    {
        if (address == 0xA10003)
            port1State = value;
        else if (address == 0xA10005)
            port2State = value;
    }

    public void WritePortUInt16(uint address, ushort value)
    {
        
    }

    public void WritePortUInt32(uint address, uint value)
    {
        
    }
}