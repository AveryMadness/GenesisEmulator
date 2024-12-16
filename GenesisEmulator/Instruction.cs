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
    
    public static class SignExtensions
    {
        public static string GetString(this Sign me)
        {
            return me is Sign.Signed ? "s" : "u";
        }
    }

    public enum ShiftDirection
    {
        Right,
        Left
    }

    public static class ShiftDirectionExtensions
    {
        public static string GetString(this ShiftDirection me)
        {
            return me is ShiftDirection.Right ? "r" : "l";
        }
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

    public static class ConditionExtensions
    {
        public static string GetString(this Condition me)
        {
            switch (me)
            {
                case Condition.True:
                    return "t";
                case Condition.False:
                    return "f";
                case Condition.High:
                    return "hi";
                case Condition.LowOrSame:
                    return "ls";
                case Condition.CarryClear:
                    return "cc";
                case Condition.CarrySet:
                    return "cs";
                case Condition.NotEqual:
                    return "ne";
                case Condition.Equal:
                    return "eq";
                case Condition.OverflowClear:
                    return "oc";
                case Condition.OverflowSet:
                    return "os";
                case Condition.Plus:
                    return "p";
                case Condition.Minus:
                    return "m";
                case Condition.GreaterThanOrEqual:
                    return "ge";
                case Condition.LessThan:
                    return "lt";
                case Condition.GreaterThan:
                    return "gt";
                case Condition.LessThanOrEqual:
                    return "le";
                default:
                    return "";
            }
        }
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
            return $"andi.b\t0x{Data:X2}, ccr";
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
            return $"andi.w\t0x{Data:X4}, sr";
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

        public override string ToString()
        {
            if (Bitnum is Immediate)
            {
                return $"andi.{Size.GetString()}\t{Bitnum.GetString()}, {Target.GetString()}";
            }
            else
            {
                return $"and.{Size.GetString()}\t{Bitnum.GetString()}, {Target.GetString()}";
            }
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

        public override string ToString()
        {
            return $"subx.{Size.GetString()}\t{Bitnum.GetString()}, {Target.GetString()}";
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

        public override string ToString()
        {
            if (Bitnum is Immediate)
            {
                return $"subi.{Size.GetString()}\t{Bitnum.GetString()}, {Target.GetString()}";
            }
            else
            {
                return $"sub.{Size.GetString()}\t{Bitnum.GetString()}, {Target.GetString()}";
            }
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

        public override string ToString()
        {
            if (Bitnum is Immediate)
            {
                return $"addi.{Size.GetString()}\t{Bitnum.GetString()}, {Target.GetString()}";
            }
            else
            {
                return $"add.{Size.GetString()}\t{Bitnum.GetString()}, {Target.GetString()}";
            }
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

        public override string ToString()
        {
            if (Bitnum is Immediate)
            {
                return $"eori.{Size.GetString()}\t{Bitnum.GetString()}, {Target.GetString()}";
            }
            else
            {
                return $"eor.{Size.GetString()}\t{Bitnum.GetString()}, {Target.GetString()}";
            }
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

        public override string ToString()
        {
            if (Bitnum is Immediate)
            {
                return $"cmpi.{Size.GetString()}\t{Bitnum.GetString()}, {Target.GetString()}";
            }
            else
            {
                return $"cmp.{Size.GetString()}\t{Bitnum.GetString()}, {Target.GetString()}";
            }
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

        public override string ToString()
        {
            return $"cmpa.{Size.GetString()}\t{Bitnum.GetString()}, {Bitnum.GetString()}, a{Reg}";
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

        public override string ToString()
        {
            return $"move.{Size.GetString()}\t{Bitnum.GetString()}, {Target.GetString()}";
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

        public override string ToString()
        {
            return $"movea.{Size.GetString()}\t{Bitnum.GetString()}, a{Register}";
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

        public override string ToString()
        {
            return $"chk.{Size.GetString()}\t{Bitnum.GetString()}, d{Register}";
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

        public override string ToString()
        {
            return $"ngex.{Size.GetString()}\t{Target.GetString()}";
        }
    }

    public class MOVEfromSR(Target.Target target) : Instruction
    {
        private Target.Target Target = target;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }

        public override string ToString()
        {
            return $"movew\t%sr, {Target.GetString()}";
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

        public override string ToString()
        {
            return $"clr.{Size.GetString()}\t{Target.GetString()}";
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

        public override string ToString()
        {
            return $"neg.{Size.GetString()}\t{Target.GetString()}";
        }
    }
    
    public class MOVEtoCCR(Target.Target target) : Instruction
    {
        private Target.Target Target = target;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }

        public override string ToString()
        {
            return $"moveb\t{Target.GetString()}, %ccr";
        }
    }
    
    public class MOVEtoSR(Target.Target target) : Instruction
    {
        private Target.Target Target = target;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }

        public override string ToString()
        {
            return $"movew\t{Target.GetString()}, %sr";
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

        public override string ToString()
        {
            return $"not.{Size.GetString()}\t{Target.GetString()}";
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
    
    public class ABCD(Target.Target target, Target.Target target2) : Instruction
    {
        private Target.Target Target = target;
        private Target.Target Target2 = target2;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }

        public override string ToString()
        {
            return $"abcd\t{Target.GetString()}, {Target2.GetString()}";
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

        public override string ToString()
        {
            return $"exg\t{Target.GetString()}, {Target2.GetString()}";
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

        public override string ToString()
        {
            return $"mul{Sign.GetString()}.w\t{Target.GetString()}, d{Reg}";
        }
    }
    
    public class SWAP(byte reg) : Instruction
    {
        private byte Reg = reg;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }

        public override string ToString()
        {
            return $"swap\td{Reg}";
        }
    }
    
    public class BKPT(byte reg) : Instruction
    {
        private byte Reg = reg;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }

        public override string ToString()
        {
            return $"bkpt\t{Reg}";
        }
    }
    
    public class PEA(Target.Target target) : Instruction
    {
        private Target.Target Target = target;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }

        public override string ToString()
        {
            return $"pea\t{Target.GetString()}";
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

        public override string ToString()
        {
            return $"tas\t{Target.GetString()}";
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

        public override string ToString()
        {
            return $"ext{(Size1 == Size.Byte && Size2 == Size.Long ? "b" : "")}.{Size2.GetString()}, d{reg}";
        }
    }
    
    public class JSR(Target.Target target) : Instruction
    {
        private Target.Target Target = target;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }

        public override string ToString()
        {
            return $"jsr\t{Target.GetString()}";
        }
    }
    
    public class JMP(Target.Target target) : Instruction
    {
        private Target.Target Target = target;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }

        public override string ToString()
        {
            return $"jmp\t{Target.GetString()}";
        }
    }

    public class TRAP(byte num) : Instruction
    {
        private byte Num = num;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }

        public override string ToString()
        {
            return $"trap\t{Num}";
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

        public override string ToString()
        {
            return $"link\ta{Reg}, {Offset:X6}";
        }
    }

    public class ULNK(byte reg) : Instruction
    {
        private byte Reg = reg;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }

        public override string ToString()
        {
            return $"ulnk\ta{Reg}";
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

        public override string ToString()
        {
            return $"movel\t{(Direction == Direction.ToTarget ? "%usp" : Target.GetString())}, {(Direction == Direction.ToTarget ? Target.GetString() : "%usp")}";
        }
    }

    public class RESET() : Instruction
    {
        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }

        public override string ToString()
        {
            return "reset";
        }
    }
    
    public class NOP() : Instruction
    {
        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }

        public override string ToString()
        {
            return "nop";
        }
    }

    public class STOP(ushort data) : Instruction
    {
        private ushort Data = data;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }

        public override string ToString()
        {
            return $"stop\t{Data:X4}";
        }
    }
    
    public class RTE() : Instruction
    {
        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }

        public override string ToString()
        {
            return "rte";
        }
    }
    
    public class RTS() : Instruction
    {
        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }

        public override string ToString()
        {
            return "rts";
        }
    }
    
    public class TRAPV() : Instruction
    {
        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }

        public override string ToString()
        {
            return $"trapv";
        }
    }
    
    public class RTR() : Instruction
    {
        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }

        public override string ToString()
        {
            return "rtr";
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

        public override string ToString()
        {
            return $"movec\t{(Direction == Direction.FromTarget ? Target.GetString() : "%vbr")}, {(Direction == Direction.FromTarget ? "%vbr": Target.GetString())}";
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

        public override string ToString()
        {
            return $"adda.{Size.GetString()}\t{Target.GetString()}, a{Reg}";
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
        
        public override string ToString()
        {
            return $"addx.{Size.GetString()}\t{SrcTarget.GetString()}, {DestTarget.GetString()}";
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
        
        public override string ToString()
        {
            return $"as{ShiftDirection.GetString()}.{Size.GetString()}\t{Count.GetString()}, {Register.GetString()}";
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

        public override string ToString()
        {
            return $"ls{ShiftDirection.GetString()}.{Size.GetString()}\t{Count.GetString()}, {Register.GetString()}";
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

        public override string ToString()
        {
            return $"rox{ShiftDirection.GetString()}.{Size.GetString()}\t{Count.GetString()}, {Register.GetString()}";
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

        public override string ToString()
        {
            return $"ro{ShiftDirection.GetString()}.{Size.GetString()}\t{Count.GetString()}, {Register.GetString()}";
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

        public override string ToString()
        {
            return $"suba.{Size.GetString()}\t{Target.GetString()}, a{Reg}";
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

        public override string ToString()
        {
            return $"db{Condition.GetString()}\td{Reg}, {Displacement:X}";
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

        public override string ToString()
        {
            return $"s{Condition.GetString()}\t{Target.GetString()}";
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
        
        public override string ToString()
        {
            return $"b{Condition.GetString()}\t{Displacement:X6}";
        }
    }
    
    public class BRA(int disp) : Instruction
    {
        private int Displacement = disp;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
        
        public override string ToString()
        {
            return $"bra\t{Displacement:X6}";
        }
    }
    
    public class BSR(int disp) : Instruction
    {
        private int Displacement = disp;

        public override bool Execute(Cpu68000 CPU)
        {
            return false;
        }
        
        public override string ToString()
        {
            return $"bra\t{Displacement:X6}";
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

        public override string ToString()
        {
            return $"moveq\t{Data:X2}, d{Reg}";
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

        public override string ToString()
        {
            return $"div{Sign.GetString()}.w\t{Target.GetString()}, d{Reg}";
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

        public override string ToString()
        {
            return $"sbcd\t{TargetX.GetString()}, {TargetY.GetString()}";
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