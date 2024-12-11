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
    private List<int> DataRegisters;
    private List<int> AddressRegisters;

    private uint ProgramCounter = 0;
    private StatusRegister StatusRegister = new();

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
        Console.WriteLine("Program Position: " + ProgramCounter);
        byte OpCode = ReadByte(ProgramCounter);

        if (!ExecuteOpcode(OpCode))
        {
            Console.WriteLine("Failed to Execute OpCode 0x" + OpCode.ToString("X") + " or OpCode requested program stop.");
            return;
        }
        
        StartExecution();
    }

    private bool ExecuteOpcode(ushort OpCode)
    {
        switch (OpCode)
        {
            //TST
            case 0x4A:
            {
                byte AddressingMode = ReadByte(ProgramCounter);

                if (AddressingMode == 0xB9)
                {
                    //Long Addressing Mode
                    uint Address = ReadInt32(ProgramCounter);
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
                else if (AddressingMode == 0x79)
                {
                    //read 16 bit from 32 bit address
                    
                    uint Address = ReadInt32(ProgramCounter);
                    short Value = (short)memory.ReadUInt16(Address);

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
            //BNE
            case 0x66:
            {
                sbyte Displacement = (sbyte)ReadByte(ProgramCounter);

                if (StatusRegister.Zero)
                {
                    ProgramCounter += (uint)Displacement;
                }

                return true;
            }
            case 0x60:
            {
                sbyte Displacement = (sbyte)ReadByte(ProgramCounter);
                
                ProgramCounter += (uint)Displacement;

                return true;
            }
            
            default:
                Console.WriteLine("Unhandled OpCode 0x" + OpCode.ToString("X"));
                return false;
        }

        return true;
    }

    private byte ReadByte(uint Address)
    {
        ProgramCounter++;
        return memory.ReadByte(Address);
    }
    
    private ushort ReadInt16(uint Address)
    {
        ProgramCounter+=2;
        return memory.ReadUInt16(Address);
    }
    
    private uint ReadInt32(uint Address)
    {
        ProgramCounter += 4;
        return memory.ReadUInt32(Address);
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