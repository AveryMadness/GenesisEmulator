using System.Diagnostics;
using System.Reflection;
using GenesisEmulator.BaseRegister;
using GenesisEmulator.Target;

namespace GenesisEmulator;

public class Decoder(MemoryManager memoryManager, uint Start)
{
    private MemoryManager _memory = memoryManager;
    private uint _start = Start;
    private ushort _instructionWord = 0;

    private const byte OPCG_BIT_OPS = 0x0;
    private const byte OPCG_MOVE_BYTE = 0x1;
    private const byte OPCG_MOVE_LONG = 0x2;
    private const byte OPCG_MOVE_WORD = 0x3;
    private const byte OPCG_MISC = 0x04;
    private const byte OPCG_ADDQ_SUBQ = 0x5;
    private const byte OPCG_BRANCH = 0x6;
    private const byte OPCG_MOVEQ = 0x7;
    private const byte OPCG_DIV_OR = 0x8;
    private const byte OPCG_SUB = 0x9;
    private const byte OPCG_ALINE = 0xA;
    private const byte OPCG_CMP_EOR = 0xB;
    private const byte OPCG_MUL_AND = 0xC;
    private const byte OPCG_ADD = 0xD;
    private const byte OPCG_SHIFT = 0xE;
    private const byte OPCG_FLINE = 0xF;

    
    public Instruction Decode()
    {
        ushort ins = ReadUInt16();
        _instructionWord = ins;

        switch ((byte)((ins & 0xF000) >> 12))
        {
            case OPCG_BIT_OPS:
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
                    byte dreg = GetHighReg(ins);
                    byte areg = GetLowReg(ins);

                    Direction dir = (ins & 0x0080) == 0 ? Direction.FromTarget : Direction.ToTarget;
                    Size size = (ins & 0x0040) == 0 ? Size.Word : Size.Long;
                    short offset = (short)ReadUInt16();

                    return new MOVEP(dreg, areg, offset, size, dir);
                }
                else if ((ins & 0x0100) == 0x100 || (ins & 0x0F00) == 0x0800)
                {
                    Target.Target bitnum = (ins & 0x0100) == 0x0100
                        ? new Target.DirectDReg(GetHighReg(ins))
                        : new Target.Immediate(ReadUInt16());

                    Target.Target target = DecodeLowerEffectiveAddress(ins, Size.Byte);
                    Size size = Size.None;

                    if (target is Target.DirectAReg || target is DirectDReg)
                    {
                        size = Size.Long;
                    }
                    else
                    {
                        size = Size.Byte;
                    }

                    switch ((ins & 0x00C0) >> 6)
                    {
                        case 0b00: return 
                    }
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

    public uint ReadUInt32()
    {
        uint value = _memory.ReadUInt32(_start);
        _start += 4;
        return value;
    }

    public byte GetHighReg(ushort ins)
    {
        return (byte)((ins & 0x0E00) >> 9);
    }

    public byte GetLowReg(ushort ins)
    {
        return (byte)(ins & 0x0007);
    }

    public byte GetHighMode(ushort ins)
    {
        return (byte)((ins & 0x01C0) >> 6);
    }

    public byte GetLowMode(ushort ins)
    {
        return (byte)((ins & 0x038) >> 3);
    }

    public Target.Target DecodeLowerEffectiveAddress(ushort ins, Size? size)
    {
        byte reg = GetLowReg(ins);
        byte mode = GetLowMode(ins);
        return GetModeAsTarget(mode, reg, size.Value);
    }

    public Target.Target DecodeUpperEffectiveAddress(ushort ins, Size? size)
    {
        byte reg = GetHighReg(ins);
        byte mode = GetHighMode(ins);
        return GetModeAsTarget(mode, reg, size.Value);
    }

    public Target.Target GetModeAsTarget(byte mode, byte reg, Size size)
    {
        switch (mode)
        {
            case 0b000: return new Target.DirectDReg(reg);
            case 0b001: return new Target.DirectAReg(reg);
            case 0b010: return new Target.IndirectAReg(reg);
            case 0b011: return new Target.IndirectARegInc(reg);
            case 0b100: return new Target.IndirectARegDec(reg);
            case 0b101:
            {
                int displacement = SignExtendToLong((uint)ReadUInt16(), Size.Word);
                return new Target.IndirectRegOffset(new AReg(reg), null, displacement);
            }
            case 0b110:
            {
                return DecodeExtensionWord(reg);
            }
            case 0b111:
            {
                switch (reg)
                {
                    case 0b000:
                    {
                        uint value = (uint)SignExtendToLong((uint)ReadUInt16(), Size.Word);
                        return new Target.IndirectMemory(value, Size.Word);
                    }
                    case 0b001:
                    {
                        uint value = ReadUInt32();
                        return new Target.IndirectMemory(value, Size.Long);
                    }
                    case 0b010:
                    {
                        int displacement = SignExtendToLong((uint)ReadUInt16(), Size.Word);
                        return new Target.IndirectRegOffset(new PC(), null, displacement);
                    }
                    case 0b011:
                    {
                        return DecodeExtensionWord(null);
                    }
                    case 0b100:
                    {
                        uint data = 0;
                        switch (size)
                        {
                            case Size.Byte:
                            {
                                data = (uint)ReadUInt16();
                                break;
                            }
                            case Size.Word:
                            {
                                data = (uint)ReadUInt16();
                                break;
                            }
                            case Size.Long:
                            {
                                data = ReadUInt32();
                                break;
                            }
                            default:
                                throw new Exception();
                        }
                        return new Target.Immediate(data);
                    }
                }
                break;
            }
        }

        throw new Exception();
    }

    public Target.Target DecodeExtensionWord(byte? areg)
    {
        ushort briefExtension = ReadUInt16();

        bool useBrief = (briefExtension & 0x0100) == 0;

        byte xregNum = (byte)((briefExtension & 0x7000) >> 12);

        XRegister.XRegister xReg = (briefExtension & 0x8000) == 0
            ? new XRegister.DReg(xregNum)
            : new XRegister.AReg(xregNum);

        Size size = (briefExtension & 0x0800) == 0 ? Size.Word : Size.Long;

        IndexRegister indexReg = new IndexRegister(xReg, scale: 0, size);
        int displacement = SignExtendToLong((uint)(briefExtension & 0x00FF), Size.Byte);

        if (areg is not null)
        {
            return new Target.IndirectRegOffset(new BaseRegister.AReg(areg.Value), indexReg, displacement);
        }
        else
        {
            return new Target.IndirectRegOffset(new BaseRegister.PC(), indexReg, displacement);
        }
    }
    
    public int SignExtendToLong(uint value, Size size)
    {
        switch (size)
        {
            case Size.Byte: return (int)(sbyte)(byte)value;
            case Size.Word: return (int)(Int16)(UInt16)value;
            case Size.Long: return (int)value;
        }
    }
}