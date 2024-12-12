using System.Net.Mail;

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
    }

    public class Instruction
    {
        public virtual bool Execute()
        {
            return false;
        }
    }

    public class ORtoCCR(byte data) : Instruction
    {
        private byte Data = data;

        public override bool Execute()
        {
            return false;
        }
    }

    public class ANDtoCCR(byte data) : Instruction
    {
        private byte Data = data;

        public override bool Execute()
        {
            return false;
        }
    }

    public class EORtoCCR(byte data) : Instruction
    {
        private byte Data = data;

        public override bool Execute()
        {
            return false;
        }
    }

    public class ORtoSR(ushort data) : Instruction
    {
        private ushort Data = data;

        public override bool Execute()
        {
            return false;
        }
    }

    public class ANDtoSR(ushort data) : Instruction
    {
        private ushort Data = data;

        public override bool Execute()
        {
            return false;
        }
    }

    public class EORtoSR(ushort data) : Instruction
    {
        private ushort Data = data;

        public override bool Execute()
        {
            return false;
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
    }
    
    
}