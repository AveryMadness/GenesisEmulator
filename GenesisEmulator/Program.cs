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