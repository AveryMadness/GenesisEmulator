namespace GenesisEmulator;

public class Controller
{
    private byte port1State = 0x00;
    private byte port2State = 0x00;

    public byte ReadPort(uint address)
    {
        return address switch
        {
            0xA10003 => port1State,
            0xA10005 => port2State,
            _ => 0xFF
        };
    }

    public void WritePort(uint address, byte value)
    {
        if (address == 0xA10003)
            port1State = value;
        else if (address == 0xA10005)
            port2State = value;
    }
}