using System.Security.Cryptography;
using GenesisEmulator.Target;

namespace GenesisEmulator
{
    public enum Direction
    {
        FromTarget,
        ToTarget
    }

    public enum Size
    {
        None,
        Byte,
        Word,
        Long
    }
    
    public static class SizeExtensions
    {
        public static string GetString(this Size me)
        {
            switch (me)
            {
                case Size.Byte:
                    return "b";
                case Size.Word:
                    return "w";
                case Size.Long:
                    return "l";
            }

            return "";
        }
    }

    public enum Sign
    {
        Signed,
        Unsigned
    }

    public enum ShiftDirection
    {
        Right,
        Left
    }

    namespace XRegister
    {
        public abstract record XRegister;

        public record DReg(byte Reg) : XRegister;

        public record AReg(byte Reg) : XRegister;
    }

    namespace BaseRegister
    {
        public abstract record BaseRegister;

        public record None : BaseRegister;

        public record PC : BaseRegister;

        public record AReg(byte Reg) : BaseRegister;
    }

    public static class BaseRegisterExtensions
    {
        public static string GetString(this BaseRegister.BaseRegister me)
        {
            if (me is BaseRegister.PC)
            {
                return "pc";
            }

            if (me is BaseRegister.AReg)
            {
                BaseRegister.AReg AReg = (BaseRegister.AReg)me;
                return $"a{AReg.Reg}";
            }

            return "";
        }
    }

    public struct IndexRegister(XRegister.XRegister reg, byte scale, Size size)
    {
        public XRegister.XRegister XReg = reg;
        public byte Scale = scale;
        public Size Size = size;
    }

    namespace RegOrImmediate
    {
        public abstract record RegOrImmediate;

        public record DReg(byte Reg) : RegOrImmediate;

        public record Immediate(byte Reg) : RegOrImmediate;
    }

    public enum ControlRegister
    {
        VBR
    }

    public enum Condition
    {
        True,
        False,
        High,
        LowOrSame,
        CarryClear,
        CarrySet,
        NotEqual,
        Equal,
        OverflowClear,
        OverflowSet,
        Plus,
        Minus,
        GreaterThanOrEqual,
        LessThan,
        GreaterThan,
        LessThanOrEqual
    }

    namespace Target
    {
        public abstract record Target;

        public record Immediate(uint Value) : Target;

        public record DirectDReg(byte Register) : Target;

        public record DirectAReg(byte Register) : Target;

        public record IndirectAReg(byte Register) : Target;

        public record IndirectARegInc(byte Register) : Target;

        public record IndirectARegDec(byte Register) : Target;

        public record IndirectRegOffset(BaseRegister.BaseRegister Base, IndexRegister? Index, int Offset) : Target;

        public record IndirectMemoryPreindexed(
            BaseRegister.BaseRegister Base,
            IndexRegister? Index,
            int Offset,
            int Displacement)
            : Target;

        public record IndirectMemoryPostindexed(
            BaseRegister.BaseRegister Base,
            IndexRegister? Index,
            int Offset,
            int Displacement)
            : Target;

        public record IndirectMemory(uint Address, Size Size) : Target;

        public static class TargetExtensions
        {
            public static string GetString(this Target target)
            {
                if (target is Immediate)
                {
                    Immediate immediate = target as Immediate;

                    return $"0x{immediate.Value:X8}";
                }
                
                if (target is DirectDReg)
                {
                    DirectDReg immediate = target as DirectDReg;

                    return $"d{immediate.Register}";
                }
                
                if (target is DirectAReg)
                {
                    DirectAReg immediate = target as DirectAReg;

                    return $"a{immediate.Register}";
                }
                
                if (target is IndirectAReg)
                {
                    IndirectAReg immediate = target as IndirectAReg;

                    return $"(a{immediate.Register})";
                }
                
                if (target is IndirectARegInc)
                {
                    IndirectARegInc immediate = target as IndirectARegInc;

                    return $"(a{immediate.Register})+";
                }
                
                if (target is IndirectARegDec)
                {
                    IndirectARegDec immediate = target as IndirectARegDec;

                    return $"-(a{immediate.Register})";
                }

                if (target is IndirectRegOffset)
                {
                    IndirectRegOffset immediate = target as IndirectRegOffset;

                    string indexStr = StringHelpers.FormatIndexDisp(immediate.Index);
                    return $"(0x{immediate.Offset:X4}, {immediate.Base.GetString()}{indexStr})";
                }

                if (target is IndirectMemoryPreindexed)
                {
                    IndirectMemoryPreindexed immediate = target as IndirectMemoryPreindexed;
                    
                    string indexStr = StringHelpers.FormatIndexDisp(immediate.Index);
                    return $"([{immediate.Base.GetString()}{indexStr}0x{immediate.Offset:X8}] + 0x{immediate.Displacement:X8})";
                }

                if (target is IndirectMemory)
                {
                    IndirectMemory immediate = target as IndirectMemory;

                    if (immediate.Size == Size.Word)
                    {
                        return $"(0x{immediate.Address:X4})";
                    }
                    else
                    {
                        return $"(0x{immediate.Address:X8})";
                    }
                }

                return "";
            }
        }
    }

    public class Instruction
    {
        public virtual bool Execute(Cpu68000 CPU)
        {
            return false;
        }

        public override string ToString()
        {
            return "Default Instruction";
        }
    }

    public class ORtoCCR(byte data) : Instruction
    {
        private byte Data = data;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }

        public override string ToString()
        {
            return $"orib\t0x{Data:X2}, ccr";
        }
    }

    public class ANDtoCCR(byte data) : Instruction
    {
        private byte Data = data;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
        
        public override string ToString()
        {
            return $"andib\t0x{Data:X2}, ccr";
        }
    }

    public class EORtoCCR(byte data) : Instruction
    {
        private byte Data = data;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
        
        public override string ToString()
        {
            return $"eorib\t0x{Data:X2}, ccr";
        }
    }

    public class ORtoSR(ushort data) : Instruction
    {
        private ushort Data = data;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
        
        public override string ToString()
        {
            return $"oriw\t0x{Data:X4}, sr";
        }
    }

    public class ANDtoSR(ushort data) : Instruction
    {
        private ushort Data = data;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
        
        public override string ToString()
        {
            return $"andiw\t0x{Data:X4}, sr";
        }
    }

    public class EORtoSR(ushort data) : Instruction
    {
        private ushort Data = data;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
        
        public override string ToString()
        {
            return $"eoriw\t0x{Data:X4}, sr";
        }
    }

    public class MOVEP(byte dreg, byte areg, short offset, Size size, Direction direction) : Instruction
    {
        private byte DReg = dreg;
        private byte AReg = areg;
        private short Offset = offset;
        private Size size = size;
        private Direction direction = direction;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
        
        public override string ToString()
        {
            switch (direction)
            {
                case Direction.ToTarget:
                {
                    return $"movep.{size.GetString()}\td{dreg}, ({areg}, a{offset})";
                }
                case Direction.FromTarget:
                {
                    return $"movep.{size.GetString()}\t({areg}, a{offset}), d{dreg}";
                }
            }

            return "";
        }
    }

    public class BTST(Target.Target bitnum, Target.Target target, Size size) : Instruction
    {
        private Target.Target Bitnum = bitnum;
        private Target.Target Target = target;
        private Size Size = size;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }

        public override string ToString()
        {
            return $"btst.{Size.GetString()}\t{Bitnum.GetString()}, {Target.GetString()}";
        }
    }
    
    public class BCHG(Target.Target bitnum, Target.Target target, Size size) : Instruction
    {
        private Target.Target Bitnum = bitnum;
        private Target.Target Target = target;
        private Size Size = size;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }

        public override string ToString()
        {
            return $"bchg.{Size.GetString()}\t{Bitnum.GetString()}, {Target.GetString()}";
        }
    }
    
    public class BCLR(Target.Target bitnum, Target.Target target, Size size) : Instruction
    {
        private Target.Target Bitnum = bitnum;
        private Target.Target Target = target;
        private Size Size = size;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
        
        public override string ToString()
        {
            return $"bclr.{Size.GetString()}\t{Bitnum.GetString()}, {Target.GetString()}";
        }
    }
    
    public class BSET(Target.Target bitnum, Target.Target target, Size size) : Instruction
    {
        private Target.Target Bitnum = bitnum;
        private Target.Target Target = target;
        private Size Size = size;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
        
        public override string ToString()
        {
            return $"bset.{Size.GetString()}\t{Bitnum.GetString()}, {Target.GetString()}";
        }
    }
    
    public class OR(Target.Target bitnum, Target.Target target, Size size) : Instruction
    {
        private Target.Target Bitnum = bitnum;
        private Target.Target Target = target;
        private Size Size = size;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }

        public override string ToString()
        {
            if (Bitnum is Immediate)
            {
                return $"ori.{Size.GetString()}\t{Bitnum.GetString()}, {Target.GetString()}";
            }
            else
            {
                return $"or.{Size.GetString()}\t{Bitnum.GetString()}, {Target.GetString()}";
            }
        }
    }
    
    public class AND(Target.Target bitnum, Target.Target target, Size size) : Instruction
    {
        private Target.Target Bitnum = bitnum;
        private Target.Target Target = target;
        private Size Size = size;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }
    
    public class SUBX(Target.Target bitnum, Target.Target target, Size size) : Instruction
    {
        private Target.Target Bitnum = bitnum;
        private Target.Target Target = target;
        private Size Size = size;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }
    
    public class SUB(Target.Target bitnum, Target.Target target, Size size) : Instruction
    {
        private Target.Target Bitnum = bitnum;
        private Target.Target Target = target;
        private Size Size = size;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }
    public class ADD(Target.Target bitnum, Target.Target target, Size size) : Instruction
    {
        private Target.Target Bitnum = bitnum;
        private Target.Target Target = target;
        private Size Size = size;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }
    
    public class EOR(Target.Target bitnum, Target.Target target, Size size) : Instruction
    {
        private Target.Target Bitnum = bitnum;
        private Target.Target Target = target;
        private Size Size = size;

        public override bool Execute(Cpu68000 CPU)  
        {
            return false;
        }
    }
    
    public class CMP(Target.Target bitnum, Target.Target target, Size size) : Instruction
    {
        private Target.Target Bitnum = bitnum;
        private Target.Target Target = target;
        private Size Size = size;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }
    
    public class CMPA(Target.Target bitnum, byte reg, Size size) : Instruction
    {
        private Target.Target Bitnum = bitnum;
        private byte Reg = reg;
        private Size Size = size;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }
    
    public class MOVE(Target.Target bitnum, Target.Target target, Size size) : Instruction
    {
        private Target.Target Bitnum = bitnum;
        private Target.Target Target = target;
        private Size Size = size;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }
    
    public class MOVEA(Target.Target bitnum, byte register, Size size) : Instruction
    {
        private Target.Target Bitnum = bitnum;
        private byte Register = register;
        private Size Size = size;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }
    
    public class CHK(Target.Target bitnum, byte register, Size size) : Instruction
    {
        private Target.Target Bitnum = bitnum;
        private byte Register = register;
        private Size Size = size;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }

    public class LEA(Target.Target target, byte register) : Instruction
    {
        private Target.Target Target = target;
        private byte Register = register;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }

        public override string ToString()
        {
            return $"lea\t{TargetExtensions.GetString(Target)}, a{Register}";
        }
    }

    public class MOVEM(Target.Target target, Size size, Direction direction, ushort data) : Instruction
    {
        private Target.Target Target = target;
        private Size Size = size;
        private Direction Direction = direction;
        private ushort Data = data;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }

        public override string ToString()
        {
            if (Direction == Direction.ToTarget)
            {
                return $"movem.{Size.GetString()}\t{StringHelpers.FormatMOVEMMask(Data, Target)}, {Target.GetString()}";
            }
            else
            {
                return $"movem.{Size.GetString()}\t{Target.GetString()}, {StringHelpers.FormatMOVEMMask(Data, Target)}";
            }
        }
    }

    public class NEGX(Target.Target target, Size size) : Instruction
    {
        public Target.Target Target = target;
        public Size Size = size;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }

    public class MOVEfromSR(Target.Target target) : Instruction
    {
        private Target.Target Target = target;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }

    public class CLR(Target.Target target, Size size) : Instruction
    {
        private Target.Target Target = target;
        private Size Size = size;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }
    
    public class NEG(Target.Target target, Size size) : Instruction
    {
        private Target.Target Target = target;
        private Size Size = size;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }
    
    public class MOVEtoCCR(Target.Target target) : Instruction
    {
        private Target.Target Target = target;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }
    
    public class MOVEtoSR(Target.Target target) : Instruction
    {
        private Target.Target Target = target;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }

    public class NOT(Target.Target target, Size size) : Instruction
    {
        private Target.Target Target = target;
        private Size Size = size;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }
    
    public class NCBD(Target.Target target) : Instruction
    {
        private Target.Target Target = target;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }
    
    public class ACBD(Target.Target target, Target.Target target2) : Instruction
    {
        private Target.Target Target = target;
        private Target.Target Target2 = target2;

        public override bool Execute(Cpu68000 CPU)
        {
            
        }
    }
    
    public class EXG(Target.Target target, Target.Target target2) : Instruction
    {
        private Target.Target Target = target;
        private Target.Target Target2 = target2;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }
    
    public class MULW(Target.Target target, byte reg, Sign sign) : Instruction
    {
        private Target.Target Target = target;
        private byte Reg = reg;
        private Sign Sign = sign;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }
    
    public class SWAP(byte reg) : Instruction
    {
        private byte Reg = reg;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }
    
    public class BKPT(byte reg) : Instruction
    {
        private byte Reg = reg;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }
    
    public class PEA(Target.Target target) : Instruction
    {
        private Target.Target Target = target;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }
    
    public class TST(Target.Target target, Size size) : Instruction
    {
        private Target.Target Target = target;
        private Size Size = size;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }

        public override string ToString()
        {
            return $"tst.{size.GetString()}\t{target.GetString()}";
        }
    }
    
    public class TAS(Target.Target target) : Instruction
    {
        private Target.Target Target = target;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }
    
    public class EXT(byte reg, Size size1, Size size2) : Instruction
    {
        private byte Reg = reg;
        private Size Size1 = size1;
        private Size Size2 = size2;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }
    
    public class JSR(Target.Target target) : Instruction
    {
        private Target.Target Target = target;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }
    
    public class JMP(Target.Target target) : Instruction
    {
        private Target.Target Target = target;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }

    public class TRAP(byte num) : Instruction
    {
        private byte Num = num;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }

    public class LINK(byte reg, Int32 offset) : Instruction
    {
        private byte Reg = reg;
        private Int32 Offset = offset;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }

    public class ULNK(byte reg) : Instruction
    {
        private byte Reg = reg;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }

    public class MOVEUSP(Target.Target target, Direction direction) : Instruction
    {
        private Target.Target Target = target;
        private Direction Direction = direction;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }

    public class RESET() : Instruction
    {
        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }
    
    public class NOP() : Instruction
    {
        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }

    public class STOP(ushort data) : Instruction
    {
        private ushort Data = data;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }
    
    public class RTE() : Instruction
    {
        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }
    
    public class RTS() : Instruction
    {
        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }
    
    public class TRAPV() : Instruction
    {
        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }
    
    public class RTR() : Instruction
    {
        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }
    
    public class MOVEC(Target.Target target, ControlRegister creg, Direction dir) : Instruction
    {
        private Target.Target Target = target;
        private ControlRegister ControlRegister = creg;
        private Direction Direction = dir;
        
        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }

    public class ADDA(Target.Target target, byte reg, Size size) : Instruction
    {
        private Target.Target Target = target;
        private byte Reg = reg;
        private Size Size = size;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }
    
    public class ADDX(Target.Target srcTarget, Target.Target destTarget, Size size) : Instruction
    {
        private Target.Target SrcTarget = srcTarget;
        private Target.Target DestTarget = destTarget;
        private Size Size = size;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }

    public class ASd(Target.Target count, Target.Target register, Size size, ShiftDirection dir) : Instruction
    {
        private Target.Target Count = count;
        private Target.Target Register = register;
        private Size Size = size;
        private ShiftDirection ShiftDirection = dir;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }
    
    public class LSd(Target.Target count, Target.Target register, Size size, ShiftDirection dir) : Instruction
    {
        private Target.Target Count = count;
        private Target.Target Register = register;
        private Size Size = size;
        private ShiftDirection ShiftDirection = dir;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }
    
    public class ROXd(Target.Target count, Target.Target register, Size size, ShiftDirection dir) : Instruction
    {
        private Target.Target Count = count;
        private Target.Target Register = register;
        private Size Size = size;
        private ShiftDirection ShiftDirection = dir;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }
    
    public class ROd(Target.Target count, Target.Target register, Size size, ShiftDirection dir) : Instruction
    {
        private Target.Target Count = count;
        private Target.Target Register = register;
        private Size Size = size;
        private ShiftDirection ShiftDirection = dir;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }
    
    public class SUBA(Target.Target target, byte reg, Size size) : Instruction
    {
        private Target.Target Target = target;
        private byte Reg = reg;
        private Size Size = size;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }
    
    //condition code?
    public class DBcc(Condition condition, byte reg, short displacement) : Instruction
    {
        private Condition Condition = condition;
        private byte Reg = reg;
        private short Displacement = displacement;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }
    
    public class Scc(Condition condition, Target.Target target) : Instruction
    {
        private Condition Condition = condition;
        private Target.Target Target = target;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }
    
    public class Bcc(Condition condition, int disp) : Instruction
    {
        private Condition Condition = condition;
        private int Displacement = disp;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }
    
    public class BRA(int disp) : Instruction
    {
        private int Displacement = disp;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }
    
    public class BSR(int disp) : Instruction
    {
        private int Displacement = disp;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }
    
    public class MOVEQ(byte data, byte reg) : Instruction
    {
        private byte Data = data;
        private byte Reg = reg;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }

    public class DIVW(Target.Target target, byte reg, Sign sign) : Instruction
    {
        private Target.Target Target = target;
        private byte Reg = reg;
        private Sign Sign = sign;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }

    public class SBCD(Target.Target targetx, Target.Target targety) : Instruction
    {
        private Target.Target TargetX = targetx;
        private Target.Target TargetY = targety;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
    }

    public class StringHelpers
    {
        public static string FormatIndexDisp(IndexRegister? index)
        {
            if (index is null)
            {
                return "";
            }
            
            string result = $", {index.Value.XReg}";

            if (index.Value.Scale != 0)
            {
                result += $"<< {index.Value.Scale}";
            }

            return result;
        }

        public static string FormatMOVEMMask(ushort mask, Target.Target target)
        {
            List<string> output = new();

            if (target is IndirectARegDec)
            {
                for (int i = 0; i < 8; i++)
                {
                    if ((mask & 0x01) != 0)
                    {
                        output.Add($"a{i}");
                    }

                    mask >>= 1;
                }

                for (int i = 0; i < 8; i++)
                {
                    if ((mask & 0x01) != 0)
                    {
                        output.Add($"d{i}");
                    }

                    mask >>= 1;
                }
            }
            else
            {
                for (int i = 0; i < 8; i++)
                {
                    if ((mask & 0x01) != 0)
                    {
                        output.Add($"d{i}");
                    }

                    mask >>= 1;
                }
                
                for (int i = 0; i < 8; i++)
                {
                    if ((mask & 0x01) != 0)
                    {
                        output.Add($"a{i}");
                    }

                    mask >>= 1;
                }
            }

            return string.Join('-', output);
        }
    }
}