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
        }
    }

    public enum Sign
    {
        Signed,
        Unsigned
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

                    return $"%d{immediate.Register}";
                }
                
                if (target is DirectAReg)
                {
                    DirectAReg immediate = target as DirectAReg;

                    return $"%a{immediate.Register}";
                }
                
                if (target is IndirectAReg)
                {
                    IndirectAReg immediate = target as IndirectAReg;

                    return $"(%a{immediate.Register})";
                }
                
                if (target is IndirectARegInc)
                {
                    IndirectARegInc immediate = target as IndirectARegInc;

                    return $"(%a{immediate.Register})+";
                }
                
                if (target is IndirectARegDec)
                {
                    IndirectARegDec immediate = target as IndirectARegDec;

                    return $"-(%a{immediate.Register})";
                }

                if (target is IndirectRegOffset)
                {
                    IndirectRegOffset immediate = target as IndirectRegOffset;

                    string indexStr = StringHelpers.FormatIndexDisp(immediate.Index);
                    return $"(0x{immediate.Offset:X4}, {immediate.Base}{indexStr})";
                }

                if (target is IndirectMemoryPreindexed)
                {
                    IndirectMemoryPreindexed immediate = target as IndirectMemoryPreindexed;
                    
                    string indexStr = StringHelpers.FormatIndexDisp(immediate.Index);
                    return $"([{immediate.Base}0x{immediate.}])";
                }
            }
        }
    }

    public class Instruction
    {
        public virtual bool Execute()
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

        public override bool Execute()
        {
            return false;
        }

        public override string ToString()
        {
            return $"orib\t0x{Data:X2}, %ccr";
        }
    }

    public class ANDtoCCR(byte data) : Instruction
    {
        private byte Data = data;

        public override bool Execute()
        {
            return false;
        }
        
        public override string ToString()
        {
            return $"andib\t0x{Data:X2}, %ccr";
        }
    }

    public class EORtoCCR(byte data) : Instruction
    {
        private byte Data = data;

        public override bool Execute()
        {
            return false;
        }
        
        public override string ToString()
        {
            return $"eorib\t0x{Data:X2}, %ccr";
        }
    }

    public class ORtoSR(ushort data) : Instruction
    {
        private ushort Data = data;

        public override bool Execute()
        {
            return false;
        }
        
        public override string ToString()
        {
            return $"oriw\t0x{Data:X4}, %sr";
        }
    }

    public class ANDtoSR(ushort data) : Instruction
    {
        private ushort Data = data;

        public override bool Execute()
        {
            return false;
        }
        
        public override string ToString()
        {
            return $"andiw\t0x{Data:X4}, %sr";
        }
    }

    public class EORtoSR(ushort data) : Instruction
    {
        private ushort Data = data;

        public override bool Execute()
        {
            return false;
        }
        
        public override string ToString()
        {
            return $"eoriw\t0x{Data:X4}, %sr";
        }
    }

    public class MOVEP(byte dreg, byte areg, short offset, Size size, Direction direction) : Instruction
    {
        private byte DReg = dreg;
        private byte AReg = areg;
        private short Offset = offset;
        private Size size = size;
        private Direction direction = direction;

        public override bool Execute()
        {
            return false;
        }
        
        public override string ToString()
        {
            switch (direction)
            {
                case Direction.ToTarget:
                {
                    return $"movel\t%usp, {}";
                }
            }
        }
    }

    public class BTST(Target.Target bitnum, Target.Target target, Size size) : Instruction
    {
        private Target.Target Bitnum = bitnum;
        private Target.Target Target = target;
        private Size Size = size;

        public override bool Execute()
        {
            return false;
        }
    }
    
    public class BCHG(Target.Target bitnum, Target.Target target, Size size) : Instruction
    {
        private Target.Target Bitnum = bitnum;
        private Target.Target Target = target;
        private Size Size = size;

        public override bool Execute()
        {
            return false;
        }
    }
    
    public class BCLR(Target.Target bitnum, Target.Target target, Size size) : Instruction
    {
        private Target.Target Bitnum = bitnum;
        private Target.Target Target = target;
        private Size Size = size;

        public override bool Execute()
        {
            return false;
        }
    }
    
    public class BSET(Target.Target bitnum, Target.Target target, Size size) : Instruction
    {
        private Target.Target Bitnum = bitnum;
        private Target.Target Target = target;
        private Size Size = size;

        public override bool Execute()
        {
            return false;
        }
    }
    
    public class OR(Target.Target bitnum, Target.Target target, Size size) : Instruction
    {
        private Target.Target Bitnum = bitnum;
        private Target.Target Target = target;
        private Size Size = size;

        public override bool Execute()
        {
            return false;
        }
    }
    
    public class AND(Target.Target bitnum, Target.Target target, Size size) : Instruction
    {
        private Target.Target Bitnum = bitnum;
        private Target.Target Target = target;
        private Size Size = size;

        public override bool Execute()
        {
            return false;
        }
    }
    
    public class SUB(Target.Target bitnum, Target.Target target, Size size) : Instruction
    {
        private Target.Target Bitnum = bitnum;
        private Target.Target Target = target;
        private Size Size = size;

        public override bool Execute()
        {
            return false;
        }
    }
    public class ADD(Target.Target bitnum, Target.Target target, Size size) : Instruction
    {
        private Target.Target Bitnum = bitnum;
        private Target.Target Target = target;
        private Size Size = size;

        public override bool Execute()
        {
            return false;
        }
    }
    
    public class EOR(Target.Target bitnum, Target.Target target, Size size) : Instruction
    {
        private Target.Target Bitnum = bitnum;
        private Target.Target Target = target;
        private Size Size = size;

        public override bool Execute()  
        {
            return false;
        }
    }
    
    public class CMP(Target.Target bitnum, Target.Target target, Size size) : Instruction
    {
        private Target.Target Bitnum = bitnum;
        private Target.Target Target = target;
        private Size Size = size;

        public override bool Execute()
        {
            return false;
        }
    }
    
    public class MOVE(Target.Target bitnum, Target.Target target, Size size) : Instruction
    {
        private Target.Target Bitnum = bitnum;
        private Target.Target Target = target;
        private Size Size = size;

        public override bool Execute()
        {
            return false;
        }
    }
    
    public class MOVEA(Target.Target bitnum, byte register, Size size) : Instruction
    {
        private Target.Target Bitnum = bitnum;
        private byte Register = register;
        private Size Size = size;

        public override bool Execute()
        {
            return false;
        }
    }
    
    public class CHK(Target.Target bitnum, byte register, Size size) : Instruction
    {
        private Target.Target Bitnum = bitnum;
        private byte Register = register;
        private Size Size = size;

        public override bool Execute()
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
            
            string result = $", %{index.Value.XReg}";

            if (index.Value.Scale != 0)
            {
                result += $"<< {index.Value.Scale}";
            }

            return result;
        }
    }
}