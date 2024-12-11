using System.Net.Mail;

namespace GenesisEmulator;

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

