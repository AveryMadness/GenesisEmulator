namespace GenesisEmulator;

public class StatusRegister
{
    public int SR; // 16-bit Status Register (stored as a 32-bit int in C#)

    // Mask values for each flag
    private const int MASK_N = 0x80; // Negative Flag (bit 7)
    private const int MASK_Z = 0x40; // Zero Flag (bit 6)
    private const int MASK_V = 0x20; // Overflow Flag (bit 5)
    private const int MASK_C = 0x10; // Carry Flag (bit 4)
    private const int MASK_X = 0x08; // Extend Flag (bit 3)

    // Interrupt priority masks (bits 0-3)
    private const int MASK_I0 = 0x01; // Interrupt level 0
    private const int MASK_I1 = 0x02; // Interrupt level 1
    private const int MASK_I2 = 0x04; // Interrupt level 2
    private const int MASK_I3 = 0x08; // Interrupt level 3

    // Supervisor flag (bit 13) and Trace (bit 15)
    private const int MASK_S = 0x2000; // Supervisor state (bit 13)
    private const int MASK_T = 0x8000; // Trace flag (bit 15)

    // Getters and Setters for Flags
    public bool Negative
    {
        get => (SR & MASK_N) != 0;
        set => SR = value ? SR | MASK_N : SR & ~MASK_N;
    }

    public bool Zero
    {
        get => (SR & MASK_Z) != 0;
        set => SR = value ? SR | MASK_Z : SR & ~MASK_Z;
    }

    public bool Overflow
    {
        get => (SR & MASK_V) != 0;
        set => SR = value ? SR | MASK_V : SR & ~MASK_V;
    }

    public bool Carry
    {
        get => (SR & MASK_C) != 0;
        set => SR = value ? SR | MASK_C : SR & ~MASK_C;
    }

    public bool Extend
    {
        get => (SR & MASK_X) != 0;
        set => SR = value ? SR | MASK_X : SR & ~MASK_X;
    }

    public bool Supervisor
    {
        get => (SR & MASK_S) != 0;
        set => SR = value ? SR | MASK_S : SR & ~MASK_S;
    }

    public bool Trace
    {
        get => (SR & MASK_T) != 0;
        set => SR = value ? SR | MASK_T : SR & ~MASK_T;
    }

    // Set Interrupt Priority Level (0-3)
    public void SetInterruptLevel(int level)
    {
        if (level < 0 || level > 3)
            throw new ArgumentException("Interrupt level must be between 0 and 3");

        SR = (SR & ~(MASK_I3 | MASK_I2 | MASK_I1 | MASK_I0)) | (1 << level);
    }

    // Display the current status register value (for debugging)
    public void DisplayStatus()
    {
        Console.WriteLine("Status Register: " + Convert.ToString(SR, 2).PadLeft(16, '0'));
    }

    public override string ToString()
    {
        return SR.ToString();
    }
}

public class Cpu68000
{
    public List<int> DataRegisters;
    public List<int> AddressRegisters;

    public uint ProgramCounter = 0;
    public StatusRegister StatusRegister = new();

    public MemoryManager memory;

    public Cpu68000(MemoryManager memory, int InitialStackPointerValue, uint EntryPoint)
    {
        DataRegisters = new() { 0, 0, 0, 0, 0, 0, 0, 0 };
        AddressRegisters = new() { 0, 0, 0, 0, 0, 0, 0, 0 };
        AddressRegisters[7] = InitialStackPointerValue;
        this.memory = memory;
        ProgramCounter = EntryPoint;
    }

    public void StartExecution()
    {
        Console.WriteLine("Program Position: " + ProgramCounter);
        
        RefWrapper<uint> RefWrapper = new(ProgramCounter);

        Decoder decoder = new Decoder(memory, RefWrapper);
        Instruction? instruction = decoder.Decode();
        Console.WriteLine($"Decoded Instruction: {instruction}");
        ProgramCounter = RefWrapper.Value;

        instruction.Execute(this);

        /*if (!ExecuteOpcode(OpCode))
        {
            Console.WriteLine("Failed to Execute OpCode 0x" + OpCode.ToString("X") + " or OpCode requested program stop.");
            return;
        }*/
        
        StartExecution();
    }

    public byte ReadByte(uint Address)
    {
        return memory.ReadByte(Address);
    }

    public void WriteByte(uint Address, byte Value)
    {
        memory.WriteByte(Address, Value);
    }
    
    public ushort ReadInt16(uint Address)
    {
        return memory.ReadUInt16(Address);
    }

    public void WriteInt16(uint Address, ushort Value)
    {
        memory.WriteUInt16(Address, Value);
    }
    
    public uint ReadInt32(uint Address)
    {
        return memory.ReadUInt32(Address);
    }

    public void WriteInt32(uint Address, uint Value)
    {
        memory.WriteUInt32(Address, Value);
    }

    public int GetStackPointer()
    {
        return AddressRegisters[7];
    }

    public int GetFramePointer()
    {
        return AddressRegisters[6];
    }

    public int GetAddressRegister(int Index)
    {
        if (Index > AddressRegisters.Count - 1 || Index < 0)
        {
            return -1;
        }

        return AddressRegisters[Index];
    }
    
    public int GetDataRegister(int Index)
    {
        if (Index > DataRegisters.Count - 1 || Index < 0)
        {
            return -1;
        }

        return DataRegisters[Index];
    }
    
    private void SetAddressRegister(int Index, int Value)
    {
        if (Index > AddressRegisters.Count - 1 || Index < 0)
        {
            return;
        }

        AddressRegisters[Index] = Value;
    }
    
    public void SetDataRegister(int Index, int Value)
    {
        if (Index > DataRegisters.Count - 1 || Index < 0)
        {
            return;
        }

        DataRegisters[Index] = Value;
    }
}