namespace GenesisEmulator;

public class Decoder(MemoryManager memoryManager, uint Start)
{
    private MemoryManager _memory = memoryManager;
    private uint _start = Start;
    private ushort _instructionWord = 0;
    
    public Instruction Decode()
    {
        ushort ins = ReadUInt16();
        _instructionWord = ins;

        switch ((byte)((ins & 0xF000) >> 12))
        {
            case 0x0:
            {
                ushort optype = (ushort)((ins & 0x0F00) >> 8);

                if ((ins & 0x13F) == 0x03C)
                {
                    switch ((ins & 0x00C0) >> 6)
                    {
                        case 0b00:
                        {
                            ushort data = ReadUInt16();

                            switch (optype)
                            {
                                case 0b0000:
                                    return new ORtoCCR((byte)data);
                                case 0b0010:
                                    return new ANDtoCCR((byte)data);
                                case 0b1010:
                                    return new EORtoCCR((byte)data);
                            }

                            break;
                        }
                        case 0b01:
                        {
                            ushort data = ReadUInt16();
                            
                            switch (optype)
                            {
                                case 0b0000: 
                                    return new ORtoSR(data);
                                case 0b0010:
                                    return new ANDtoSR(data);
                                case 0b1010:
                                    return new EORtoSR(data);
                            }

                            break;
                        }
                    }
                }
                else if (((byte)ins & 0x138) == 0x108)
                {
                    byte dreg = (byte)((ins & 0x0E00) >> 9);
                    byte areg = (byte)(ins & 0x0007);
                    
                    
                }
            }
        }
    }
    
    
    public ushort ReadUInt16()
    {
        ushort value = _memory.ReadUInt16(_start);
        _start += 2;
        return value;
    }
}