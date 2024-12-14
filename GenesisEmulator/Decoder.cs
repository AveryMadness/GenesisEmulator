using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using GenesisEmulator.BaseRegister;
using GenesisEmulator.Target;
using Microsoft.Win32.SafeHandles;

namespace GenesisEmulator;

public class RefWrapper<T>
{
    public T Value { get; set; }

    public RefWrapper(T value)
    {
        Value = value;
    }
}

public class Decoder(MemoryManager memoryManager, RefWrapper<uint> Start)
{
    private MemoryManager _memory = memoryManager;
    private RefWrapper<uint> _start = Start;
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

    
    public Instruction? Decode()
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
                        case 0b00: return new BTST(bitnum, target, size);
                        case 0b01: return new BCHG(bitnum, target, size);
                        case 0b10: return new BCLR(bitnum, target, size);
                        case 0b11: return new BSET(bitnum, target, size);
                    }
                }
                else
                {
                    Size size = GetSize(ins);
                    uint data = 0;
                    switch (size)
                    {
                        case Size.Byte: data = (uint)ReadUInt16() & 0xFF;
                            break;
                        case Size.Word: data = (uint)ReadUInt16();
                            break;
                        case Size.Long: data = ReadUInt32();
                            break;
                        default: throw new Exception();
                    }

                    Target.Target target = DecodeLowerEffectiveAddress(ins, size);

                    switch (optype)
                    {
                        case 0b0000: return new OR(new Immediate(data), target, size);
                        case 0b0010: return new AND(new Immediate(data), target, size);
                        case 0b0100: return new SUB(new Immediate(data), target, size);
                        case 0b0110: return new ADD(new Immediate(data), target, size);
                        case 0b1010: return new EOR(new Immediate(data), target, size);
                        case 0b1100: return new CMP(new Immediate(data), target, size);
                    }
                }

                break;
            }

            case OPCG_MOVE_BYTE:
            {
                Target.Target src = DecodeLowerEffectiveAddress(ins, Size.Byte);
                Target.Target dest = DecodeUpperEffectiveAddress(ins, Size.Byte);
                return new MOVE(src, dest, Size.Byte);
            }

            case OPCG_MOVE_LONG:
            {
                Target.Target src = DecodeLowerEffectiveAddress(ins, Size.Long);
                Target.Target dest = DecodeUpperEffectiveAddress(ins, Size.Long);

                if (dest is DirectAReg)
                {
                    DirectAReg reg = (DirectAReg)dest;
                    return new MOVEA(src, reg.Register, Size.Long);
                }

                return new MOVE(src, dest, Size.Long);
            }

            case OPCG_MOVE_WORD:
            {
                Target.Target src = DecodeLowerEffectiveAddress(ins, Size.Word);
                Target.Target dest = DecodeUpperEffectiveAddress(ins, Size.Word);

                if (dest is DirectAReg)
                {
                    DirectAReg reg = (DirectAReg)dest;
                    return new MOVEA(src, reg.Register, Size.Word);
                }

                return new MOVE(src, dest, Size.Word);
            }
            
            case OPCG_MISC:
            {
                ushort ins_0f00 = (ushort)(ins & 0x0F00);
                ushort ins_00f0 = (ushort)(ins & 0x00F0);

                if ((ins & 0x180) == 0x180)
                {
                    if ((ins & 0x040) == 0)
                    {
                        Size size = GetSize(ins);

                        if (size is Size.Long)
                            size = Size.Word;

                        byte reg = GetHighReg(ins);

                        Target.Target target = DecodeLowerEffectiveAddress(ins, size);
                        return new CHK(target, reg, size);
                    }
                    else
                    {
                        Target.Target src = DecodeLowerEffectiveAddress(ins, null);
                        byte dest = GetHighReg(ins);
                        return new LEA(src, dest);
                    }
                }
                else if ((ins & 0xB80) == 0x880 && (ins & 0x038) != 0)
                {
                    Size size = (ins & 0x0040) == 0 ? Size.Word : Size.Long;
                    ushort data = ReadUInt16();
                    Target.Target target = DecodeLowerEffectiveAddress(ins, null);
                    Direction dir = (ins & 0x0400) == 0 ? Direction.ToTarget : Direction.FromTarget;
                    return new MOVEM(target, size, dir, data);
                }
                else if ((ins & 0x800) == 0)
                {
                    Target.Target target = DecodeLowerEffectiveAddress(ins, Size.Word);
                    Size size = GetSize(ins);

                    switch ((ins & 0x0700) >> 8)
                    {
                        
                        case 0b000:
                        {
                            switch (size)
                            {
                                case Size.None:
                                    return new MOVEfromSR(target);
                                default:
                                    return new NEGX(target, size);
                            }
                        }
                        case 0b010:
                        {
                            switch (size)
                            {
                                case Size.None:
                                    throw new Exception();
                                default:
                                    return new CLR(target, size);
                            }
                        }
                        case 0b100:
                        {
                            switch (size)
                            {
                                case Size.None:
                                    return new NEG(target, size);
                                default:
                                    return new MOVEtoCCR(target);
                            }
                        }
                        case 0b110:
                        {
                            switch (size)
                            {
                                case Size.None:
                                    return new MOVEtoSR(target);
                                default:
                                    return new NOT(target, size);
                            }
                        }

                        default:
                        {
                            throw new Exception();
                        }
                    }
                }
                else if (ins_0f00 == 0x800 || ins_0f00 == 0x900)
                {
                    byte opcode = (byte)((ins & 0x01C0) >> 6);
                    byte mode = GetLowMode(ins);

                    switch ((opcode, mode))
                    {
                        case (0b000, _):
                        {
                            Target.Target target = DecodeLowerEffectiveAddress(ins, Size.Byte);
                            return new NCBD(target);
                        }

                        case (0b001, 0b000):
                        {
                            return new SWAP(GetLowReg(ins));
                        }

                        case (0b001, 0b001):
                        {
                            return new BKPT(GetLowReg(ins));
                        }

                        case (0b001, _):
                        {
                            Target.Target target = DecodeLowerEffectiveAddress(ins, null);
                            return new PEA(target);
                        }

                        case (0b010, 0b000):
                        {
                            return new EXT(GetLowReg(ins), Size.Byte, Size.Word);
                        }

                        case (0b011, 0b000):
                        {
                            return new EXT(GetLowReg(ins), Size.Word, Size.Long);
                        }

                        case (0b111, 0b000):
                        {
                            return new EXT(GetLowReg(ins), Size.Byte, Size.Long);
                        }
                        
                        default:
                            throw new Exception();
                    }
                }
                else if (ins_0f00 == 0xA00)
                {
                    if ((ins & 0x0FF) == 0xFC)
                    {
                        throw new Exception();
                    }
                    else
                    {
                        Target.Target target = DecodeLowerEffectiveAddress(ins, Size.Word);

                        Size size = GetSize(ins);

                        switch (size)
                        {
                            case Size.None:
                                return new TAS(target);
                            default:
                                return new TST(target, size);
                        }
                    }
                }
                else if (ins_0f00 == 0xE00)
                {
                    if ((ins & 0x80) == 0x80)
                    {
                        Target.Target target = DecodeLowerEffectiveAddress(ins, null);
                        if ((ins & 0b01000000) == 0)
                        {
                            return new JSR(target);
                        }
                        else
                        {
                            return new JMP(target);
                        }
                    }
                    else if (ins_00f0 == 0x40)
                    {
                        return new TRAP((byte)(ins & 0x000F));
                    }
                    else if (ins_00f0 == 0x50)
                    {
                        byte reg = GetLowReg(ins);

                        if ((ins & 0b1000) == 0)
                        {
                            int data = (int)(ReadUInt16());
                            return new LINK(reg, data);
                        }
                        else
                        {
                            return new ULNK(reg);
                        }
                    }
                    else if (ins_00f0 == 0x60)
                    {
                        byte reg = GetLowReg(ins);
                        Direction direction = (ins & 0b1000) == 0 ? Direction.FromTarget : Direction.ToTarget;
                        return new MOVEUSP(new DirectAReg(reg), direction);
                    }
                    else
                    {
                        switch (ins & 0x00FF)
                        {
                            case 0x70: return new RESET();
                            case 0x71: return new NOP();
                            case 0x72:
                            {
                                ushort data = ReadUInt16();
                                return new STOP(data);
                            }
                            case 0x73: return new RTE();
                            case 0x75: return new RTS();
                            case 0x76: return new TRAPV();
                            case 0x77: return new RTR();
                            case 0x7A:
                            {
                                Direction direction = (ins & 0x01) == 0 ? Direction.ToTarget : Direction.FromTarget;
                                ushort ins2 = ReadUInt16();
                                Target.Target target = null;

                                if ((ins2 & 0x8000) == 0)
                                {
                                    target = new DirectDReg((byte)((ins2 & 0x7000) >> 12));
                                }
                                else
                                {
                                    target = new DirectAReg((byte)((ins2 & 0x7000) >> 12));
                                }

                                ControlRegister cref = ControlRegister.VBR;
                                if ((ins2 & 0xFFF) != 0x801)
                                {
                                    throw new Exception();
                                }

                                return new MOVEC(target, cref, direction);
                            }
                            default:
                                throw new Exception();
                        }
                    }
                }

                break;
            }
            case OPCG_ADDQ_SUBQ:
            {
                Size size = GetSize(ins);

                if (size is not Size.None)
                {
                    Target.Target target = DecodeLowerEffectiveAddress(ins, size);
                    uint data = (uint)((ins & 0x0E00) >> 9);

                    if (data == 0)
                        data = 8;

                    if (target is DirectAReg)
                    {
                        DirectAReg reg = (DirectAReg)target;

                        if ((ins & 0x0100) == 0)
                        {
                            return new ADDA(new Immediate(data), reg.Register, size);
                        }
                        else
                        {
                            return new SUBA(new Immediate(data), reg.Register, size);
                        }
                    }
                    else
                    {
                        if ((ins & 0x0100) == 0)
                        {
                            return new ADD(new Immediate(data), target, size);
                        }
                        else
                        {
                            return new SUB(new Immediate(data), target, size);
                        }
                    }
                }
                else
                {
                    byte mode = GetLowMode(ins);
                    Condition condition = GetCondition(ins);

                    if (mode == 0b001)
                    {
                        byte reg = GetLowReg(ins);
                        short disp = (short)ReadUInt16();
                        return new DBcc(condition, reg, disp);
                    }
                    else
                    {
                        Target.Target target = DecodeLowerEffectiveAddress(ins, Size.Byte);
                        return new Scc(condition, target);
                    }
                }
            }

            case OPCG_BRANCH:
            {
                int disp = (int)(ins & 0xFF);
                if (disp == 0)
                {
                    disp = (int)ReadUInt16();
                }

                Condition condition = GetCondition(ins);

                switch (condition)
                {
                    case Condition.True:
                        return new BRA(disp);
                    case Condition.False:
                        return new BSR(disp);
                    default:
                        return new Bcc(condition, disp);
                }
            }
            case OPCG_MOVEQ:
            {
                if ((ins & 0x0100) != 0)
                {
                    throw new Exception();
                }

                byte reg = GetHighReg(ins);
                byte data = (byte)(ins & 0xFF);
                return new MOVEQ(data, reg);
            }
            case OPCG_DIV_OR:
            {
                Size size = GetSize(ins);
                
                if (size == Size.None)
                {
                    Sign sign = (ins & 0x0100) == 0 ? Sign.Unsigned : Sign.Signed;
                    Target.Target effective_addr = DecodeLowerEffectiveAddress(ins, Size.Word);
                    return new DIVW(effective_addr, GetHighReg(ins), sign);
                }
                else if ((ins & 0x1F0) == 0x100)
                {
                    byte regx = GetHighReg(ins);
                    byte regy = GetLowReg(ins);

                    if ((ins & 0x08) != 0)
                    {
                        return new SBCD(new IndirectARegDec(regy), new IndirectARegDec(regx));
                    }
                    else
                    {
                        return new SBCD(new DirectDReg(regy), new DirectDReg(regx));
                    }
                }
                else
                {
                    var data_reg = new DirectDReg(GetHighReg(ins));
                    Target.Target effective_addr = DecodeLowerEffectiveAddress(ins, size);
                    Target.Target from;
                    Target.Target to;
                    if ((ins & 0x0100) == 0)
                    {
                        from = effective_addr;
                        to = data_reg;
                    }
                    else
                    {
                        from = data_reg;
                        to = effective_addr;
                    }

                    return new OR(from, to, size);
                }
            }
            case OPCG_SUB:
            {
                byte reg = GetHighReg(ins);
                ushort dir = (ushort)((ins & 0x0100) >> 8);
                Size size = GetSize(ins);

                if (size != Size.None)
                {
                    if ((ins & 0b100110000) == 0b100000000)
                    {
                        byte src = GetLowReg(ins);
                        byte dest = GetHighReg(ins);

                        if ((ins & 0x08) == 0)
                        {
                            return new SUBX(new DirectDReg(src), new DirectDReg(dest), size);
                        }
                        else
                        {
                            return new SUBX(new IndirectARegDec(src), new IndirectARegDec(dest), size);
                        }
                    }
                    else
                    {
                        Target.Target target = DecodeLowerEffectiveAddress(ins, size);
                        if (dir == 0)
                        {
                            return new SUB(target, new DirectDReg(reg), size);
                        }
                        else
                        {
                            return new SUB(new DirectDReg(reg), target, size);
                        }
                    }
                }
                else
                {
                    size = dir == 0 ? Size.Word : Size.Long;
                    Target.Target target = DecodeLowerEffectiveAddress(ins, size);
                    return new SUBA(target, reg, size);
                }
            }
            case OPCG_CMP_EOR:
            {
                byte reg = GetHighReg(ins);
                ushort optype = (ushort)((ins & 0x0100) >> 8);
                Size size = GetSize(ins);

                if (size != Size.None)
                {
                    if (optype == 0b1)
                    {
                        if (GetLowMode(ins) == 0b001)
                        {
                            return new CMP(new IndirectARegInc(GetLowReg(ins)), new IndirectARegInc(reg), size);
                        }
                        else
                        {
                            Target.Target target = DecodeLowerEffectiveAddress(ins, size);
                            return new EOR(new DirectDReg(reg), target, size);
                        }
                    }
                    else if (optype == 0b0)
                    {
                        Target.Target target = DecodeLowerEffectiveAddress(ins, size);
                        return new CMP(target, new DirectDReg(reg), size);
                    }
                }
                else
                {
                    size = optype == 0 ? Size.Word : Size.Long;
                    Target.Target target = DecodeLowerEffectiveAddress(ins, size);
                    return new CMPA(target, reg, size);
                }

                throw new Exception();
            }
            case OPCG_MUL_AND:
            {
                Size size = GetSize(ins);

                if ((ins & 0b0001_1111_0000) == 0b0001_0000_0000)
                {
                    byte regx = GetHighReg(ins);
                    byte regy = GetLowReg(ins);

                    if ((ins & 0x08) != 0)
                    {
                        return new ACBD(new IndirectARegDec(regy), new IndirectARegDec(regx));
                    }
                    else
                    {
                        return new ACBD(new DirectDReg(regy), new DirectDReg(regx));
                    }
                }
                else if ((ins & 0b0001_0011_0000) == 0b0001_0000_0000 && size != Size.None)
                {
                    byte regx = GetHighReg(ins);
                    byte regy = GetLowReg(ins);

                    switch ((ins & 0x00F8) >> 3)
                    {
                        case 0b01000: return new EXG(new DirectDReg(regx), new DirectDReg(regy));
                        case 0b01001: return new EXG(new DirectAReg(regx), new DirectAReg(regy));
                        case 0b10001: return new EXG(new DirectDReg(regx), new DirectAReg(regy));
                        default: throw new Exception();
                    }
                }
                else if (size == Size.None)
                {
                    Sign sign = ((ins & 0x0100) == 0) ? Sign.Unsigned : Sign.Signed;
                    Target.Target effectiveAddr = DecodeLowerEffectiveAddress(ins, Size.Word);
                    return new MULW(effectiveAddr, GetHighReg(ins), sign);
                }
                else
                {
                    var DataReg = new DirectDReg(GetHighReg(ins));
                    Target.Target effectiveAddr = DecodeLowerEffectiveAddress(ins, size);
                    Target.Target from;
                    Target.Target to;

                    if ((ins & 0x0100) == 0)
                    {
                        from = effectiveAddr;
                        to = DataReg;
                    }
                    else
                    {
                        to = effectiveAddr;
                        from = DataReg;
                    }

                    return new AND(from, to, size);
                }
            }
            case OPCG_ADD:
            {
                byte reg = GetHighReg(ins);
                ushort dir = (ushort)((ins & 0x0100) >> 8);
                Size size = GetSize(ins);

                if (size != Size.None)
                {
                    if ((ins & 0b100110000) == 0b100000000)
                    {
                        byte src = GetLowReg(ins);
                        byte dest = GetHighReg(ins);

                        if ((ins & 0x08) == 0)
                        {
                            return new ADDX(new DirectDReg(src), new DirectDReg(dest), size);
                        }
                        else
                        {
                            return new ADDX(new IndirectARegDec(src), new IndirectARegDec(dest), size);
                        }
                    }
                    else
                    {
                        Target.Target target = DecodeLowerEffectiveAddress(ins, size);

                        if (dir == 0)
                        {
                            return new ADD(target, new DirectDReg(reg), size);
                        }
                        else
                        {
                            return new ADD(new DirectDReg(reg), target, size);
                        }
                    }
                }
                else
                {
                    size = dir == 0 ? Size.Word : Size.Long;
                    Target.Target target = DecodeLowerEffectiveAddress(ins, size);
                    return new ADDA(target, reg, size);
                }
            }
            case OPCG_SHIFT:
            {
                ShiftDirection dir = (ins & 0x0100) == 0 ? ShiftDirection.Right : ShiftDirection.Left;

                Size size = GetSize(ins);

                if (size != Size.None)
                {
                    byte reg = GetLowReg(ins);
                    byte rotation = GetHighReg(ins);
                    Target.Target count = (ins & 0x0020) == 0
                        ? new Immediate(rotation != 0 ? (uint)rotation : 8)
                        : new DirectDReg(rotation);

                    switch ((ins & 0x0018) >> 3)
                    {
                        case 0b00: return new ASd(count, new DirectDReg(reg), size, dir);
                        case 0b01: return new LSd(count, new DirectDReg(reg), size, dir);
                        case 0b10: return new ROXd(count, new DirectDReg(reg), size, dir);
                        case 0b11: return new ROd(count, new DirectDReg(reg), size, dir);
                        default: throw new Exception();
                    }
                }
                else
                {
                    if ((ins & 0x800) == 0)
                    {
                        Target.Target target = DecodeLowerEffectiveAddress(ins, Size.Word);
                        var count = new Immediate(1);
                        switch ((ins & 0x0600) >> 9)
                        {
                            case 0b00: return new ASd(count, target, Size.Word, dir);
                            case 0b01: return new LSd(count, target, Size.Word, dir);
                            case 0b10: return new ROXd(count, target, Size.Word, dir);
                            case 0b11: return new ROd(count, target, Size.Word, dir);
                            default: throw new Exception();
                        }
                    }
                }

                throw new Exception();
            }
        }
        return null;
    }
    
    
    public ushort ReadUInt16()
    {
        ushort value = _memory.ReadUInt16(_start.Value);
        _start.Value += 2;
        return value;
    }

    public uint ReadUInt32()
    {
        uint value = _memory.ReadUInt32(_start.Value);
        _start.Value += 4;
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
        return GetModeAsTarget(mode, reg, size);
    }

    public Target.Target DecodeUpperEffectiveAddress(ushort ins, Size? size)
    {
        byte reg = GetHighReg(ins);
        byte mode = GetHighMode(ins);
        return GetModeAsTarget(mode, reg, size.Value);
    }

    public Target.Target GetModeAsTarget(byte mode, byte reg, Size? size)
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
            default: return 0;
        }
    }

    public Size GetSize(ushort ins)
    {
        switch ((ins & 0x00C0) >> 6)
        {
            case 0b00: return Size.Byte;
            case 0b01: return Size.Word;
            case 0b10: return Size.Long;
            default: return Size.None;
        }
    }

    public Condition GetCondition(ushort ins)
    {
        switch ((ins & 0x0F00) >> 8)
        {
            case 0b0000: return Condition.True;
            case 0b0001: return Condition.False;
            case 0b0010: return Condition.High;
            case 0b0011: return Condition.LowOrSame;
            case 0b0100: return Condition.CarryClear;
            case 0b0101: return Condition.CarrySet;
            case 0b0110: return Condition.NotEqual;
            case 0b0111: return Condition.Equal;
            case 0b1000: return Condition.OverflowClear;
            case 0b1001: return Condition.OverflowSet;
            case 0b1010: return Condition.Plus;
            case 0b1011: return Condition.Minus;
            case 0b1100: return Condition.GreaterThanOrEqual;
            case 0b1101: return Condition.LessThan;
            case 0b1110: return Condition.GreaterThan;
            case 0b1111: return Condition.LessThanOrEqual;
            default: return Condition.True;
        }
    }
}