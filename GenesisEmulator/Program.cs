// See https://aka.ms/new-console-template for more information

using GenesisEmulator;

public class Program
{
    public static void Main(string[] args)
    {
        byte[] fileBytes = File.ReadAllBytes("rom.md");
        MemoryStream stream = new MemoryStream(fileBytes);

        Rom rom = Rom.ReadFromStream(stream);
        Console.WriteLine("Hardware Designation: " + rom.HardwareDesignation);
        Console.WriteLine("Copyright Info: " + rom.Copyright);
        Console.WriteLine("Domestic Title: " + rom.DomesticGameTitle);
        Console.WriteLine("Overseas Title: " + rom.OverseasGameTitle);
        Console.WriteLine("Product Code: " + rom.ProductInfo);
        Console.WriteLine("Checksum Bytes (16 Bits): " + BytesToHexString(rom.Checksum));
        Console.WriteLine("I/O Info: " + rom.IOInfo);
        Console.WriteLine("ROM Start Address: " + rom.ROMStart);
        Console.WriteLine("ROM End Address: " + rom.ROMEnd);
        Console.WriteLine("RAM Start Address: " + rom.RAMStart);
        Console.WriteLine("RAM End Address: " + rom.RAMEnd);
        Console.WriteLine("Compatible Regions: " + GetAllRegionNames(rom.Regions));
        Console.WriteLine("Entry Point: " + rom.EntryPoint);

        Controller controller = new Controller();

        MemoryManager memoryManager = new MemoryManager();
        //rom memory
        memoryManager.AddMemoryRegion(0x000000, 0x3FFFFF, new RomHandler(rom.Code));
        //Z80 memory
        memoryManager.AddMemoryRegion(0xA00000, 0xA0FFFF, new Ram(0x10000));
        //I/O registers
        memoryManager.AddMemoryRegion(0xA10000, 0xA1001F, new IoHandler(controller));
        //VDP registers, placeholder size
        memoryManager.AddMemoryRegion(0xC00000, 0xC0001F, new Ram(0x20));
        //Work RAM
        memoryManager.AddMemoryRegion(0xFF0000, 0xFFFFFF, new Ram(0x10000));

        Cpu68000 CPU = new Cpu68000(memoryManager, rom.InitialStackPointerValue, (uint)rom.EntryPoint - 0x200);
        CPU.StartExecution();
    }

    public static string BytesToHexString(byte[] bytes)
    {
        return BitConverter.ToString(bytes).Replace("-", " ");
    }
    
    public static string GetAllRegionNames(Region regions)
    {
        string final = string.Empty;
        foreach (Region region in Enum.GetValues(typeof(Region)))
        {
            if (region != Region.Invalid && regions.HasFlag(region))
            {
                final += region + " ";
            }
        }

        return final;
    }
}