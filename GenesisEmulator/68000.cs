namespace GenesisEmulator;

public class StatusRegister
{
    private int SR; // 16-bit Status Register (stored as a 32-bit int in C#)

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
    private List<int> DataRegisters = new(8);
    private List<int> AddressRegisters = new(8);

    private uint ProgramCounter = 0;
    private StatusRegister StatusRegister;

    private MemoryManager memory;

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
        while (true)
        {
            Console.WriteLine("Program Position: " + ProgramCounter);
            byte OpCode = memory.ReadByte((uint)ProgramCounter++);

            if (!ExecuteOpcode(OpCode))
            {
                Console.WriteLine("Failed to Execute OpCode 0x" + OpCode.ToString("X"));
                return;
            }
        }
    }

    private bool ExecuteOpcode(byte OpCode)
    {
        switch (OpCode)
        {
            case 0x4A:
            {
                byte AddressingMode = memory.ReadByte((uint)ProgramCounter++);

                if (AddressingMode == 0xB9)
                {
                    //Long Addressing Mode
                    uint Address = memory.ReadUInt32((uint)ProgramCounter++);
                    int Value = (int)memory.ReadUInt32(Address);

                    if (Value == 0)
                    {
                        StatusRegister.Zero = true;
                        StatusRegister.Negative = false;
                    }
                    else if (Value < 0)
                    {
                        StatusRegister.Negative = true;
                        StatusRegister.Zero = false;
                    }
                    else
                    {
                        StatusRegister.Negative = false;
                        StatusRegister.Zero = false;
                    }
                }
                else if (AddressingMode == 0xA9)
                {
                    //Short Addressing Mode
                    ushort Address = memory.ReadUInt16(ProgramCounter++);
                    
                    short Value = (short)memory.ReadUInt16((uint)Address);

                    if (Value == 0)
                    {
                        StatusRegister.Zero = true;
                        StatusRegister.Negative = false;
                    }
                    else if (Value < 0)
                    {
                        StatusRegister.Negative = true;
                        StatusRegister.Zero = false;
                    }
                    else
                    {
                        StatusRegister.Negative = false;
                        StatusRegister.Zero = false;
                    }
                }
                else if (AddressingMode == 0xA0)
                {
                    //Absolute Addressing Mode
                    byte AbsoluteAddress = memory.ReadByte((uint)ProgramCounter++);
                    
                    sbyte Value = (sbyte)memory.ReadByte(AbsoluteAddress);

                    if (Value == 0)
                    {
                        StatusRegister.Zero = true;
                        StatusRegister.Negative = false;
                    }
                    else if (Value < 0)
                    {
                        StatusRegister.Negative = true;
                        StatusRegister.Zero = false;
                    }
                    else
                    {
                        StatusRegister.Negative = false;
                        StatusRegister.Zero = false;
                    }
                }
                else
                {
                    Console.WriteLine("Unhandled Addressing Mode 0x" + AddressingMode.ToString("X"));
                    return false;
                }
                
                break;
            }
            
            default:
                Console.WriteLine("Unhandled OpCode 0x" + OpCode.ToString("X"));
                return false;
        }

        return true;
    }
    
    private Int16 ReadInt16(uint Address)
    {
        return (short)((memory.ReadByte(Address) << 8) | memory.ReadByte(Address + 1));
    }
    
    private int ReadInt32(uint Address)
    {
        return (ReadInt16(Address) << 16) | (ReadInt16(Address + 2) & 0xFFFF);
    }

    private int GetStackPointer()
    {
        return AddressRegisters[7];
    }

    private int GetFramePointer()
    {
        return AddressRegisters[6];
    }

    private int GetAddressRegister(int Index)
    {
        if (Index > AddressRegisters.Count - 1 || Index < 0)
        {
            return -1;
        }

        return AddressRegisters[Index];
    }
    
    private int GetDataRegister(int Index)
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
    
    private void SetDataRegister(int Index, int Value)
    {
        if (Index > DataRegisters.Count - 1 || Index < 0)
        {
            return;
        }

        DataRegisters[Index] = Value;
    }
}