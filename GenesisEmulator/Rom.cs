using System.Text;

namespace GenesisEmulator;

[Flags]
public enum Region
{
    Invalid = 0,
    Japan = 1,
    Americas = 2,
    Europe = 4,
}

public class Rom
{
    public string HardwareDesignation = string.Empty;
    public string Copyright = string.Empty;
    public string DomesticGameTitle = string.Empty;
    public string OverseasGameTitle = string.Empty;
    public string ProductInfo = string.Empty;
    
    public byte[] Checksum = Array.Empty<byte>();
    
    public string IOInfo = string.Empty;
    
    public Int32 ROMStart = 0;
    public Int32 ROMEnd = 0;
    public Int32 RAMStart = 0;
    public Int32 RAMEnd = 0;

    public string ExtraMemoryMagic = string.Empty;
    public byte ExtraMemoryRamType = 0;
    public Int32 ExtraMemoryStart = 0;
    public Int32 ExtraMemoryEnd = 0;
    
    public string ModemInfo = string.Empty;
    
    public Region Regions = Region.Invalid;

    public static Rom ReadFromStream(Stream stream)
    {
        var rom = new Rom();
        
        //100 bytes of garbage at the beginning? perhaps a signature
        stream.Seek(0x100, SeekOrigin.Begin);
        
        byte[] HardwareDesignationBuffer = new byte[16];
        stream.Read(HardwareDesignationBuffer, 0, HardwareDesignationBuffer.Length);
        rom.HardwareDesignation = ReadPaddedString(HardwareDesignationBuffer);
        
        byte[] CopyrightBuffer = new byte[0x10];
        stream.Read(CopyrightBuffer, 0, CopyrightBuffer.Length);
        rom.Copyright = ReadPaddedString(CopyrightBuffer);
        
        byte[] DomesticTitleBuffer = new byte[0x30];
        stream.Read(DomesticTitleBuffer, 0, DomesticTitleBuffer.Length);
        rom.DomesticGameTitle = ReadPaddedString(DomesticTitleBuffer);
        
        byte[] OverseasTitleBuffer = new byte[0x30];
        stream.Read(OverseasTitleBuffer, 0, OverseasTitleBuffer.Length);
        rom.OverseasGameTitle = ReadPaddedString(OverseasTitleBuffer);
        
        byte[] ProductInfoBuffer = new byte[0xE];
        stream.Read(ProductInfoBuffer, 0, ProductInfoBuffer.Length);
        rom.ProductInfo = ReadPaddedString(ProductInfoBuffer);

        byte[] ChecksumBuffer = new byte[0x2];
        stream.Read(ChecksumBuffer, 0, 2);
        rom.Checksum = ChecksumBuffer;
        
        byte[] IOInfoBuffer = new byte[0x10];
        stream.Read(IOInfoBuffer, 0, IOInfoBuffer.Length);
        rom.IOInfo = ReadPaddedString(IOInfoBuffer);

        byte[] ROMStartBuffer = new byte[0x4];
        stream.Read(ROMStartBuffer, 0, ROMStartBuffer.Length);
        rom.ROMStart = BitConverter.ToInt32(ROMStartBuffer.Reverse().ToArray());
        
        byte[] ROMEndBuffer = new byte[0x4];
        stream.Read(ROMEndBuffer, 0, ROMEndBuffer.Length);
        rom.ROMEnd = BitConverter.ToInt32(ROMEndBuffer.Reverse().ToArray());
        
        byte[] RAMStartBuffer = new byte[0x4];
        stream.Read(RAMStartBuffer, 0, RAMStartBuffer.Length);
        rom.RAMStart = BitConverter.ToInt32(RAMStartBuffer.Reverse().ToArray());
        
        byte[] RAMEndBuffer = new byte[0x4];
        stream.Read(RAMEndBuffer, 0, RAMEndBuffer.Length);
        rom.RAMEnd = BitConverter.ToInt32(RAMEndBuffer.Reverse().ToArray());
        
        stream.Seek(0x1F0, SeekOrigin.Begin);
        
        byte[] RegionsBuffer = new byte[0x3];
        stream.Read(RegionsBuffer, 0, RegionsBuffer.Length);
        string regionStr = ReadPaddedString(RegionsBuffer);

        rom.Regions = ReadRegion(regionStr);
        
        return rom;
    }

    public static Region ReadRegion(string Regions)
    {
        Region finalRegion = Region.Invalid;
        foreach (char character in Regions)
        {
            if (character == 'J')
                finalRegion |= Region.Japan;
            if (character == 'U')
                finalRegion |= Region.Americas;
            if (character == 'E')
                finalRegion |= Region.Europe;
        }

        return finalRegion;
    }

    public static string ReadPaddedString(byte[] buffer)
    {
        string finalString = "";

        for (int i = 0; i < buffer.Length; i++)
        {
            byte b = buffer[i];
            if (b == 0x20)
            {
                //check to see if its padding, or just a space
                if (i == buffer.Length - 1)
                {
                    //its the end anyway, just ignore it
                    continue;
                }

                if (buffer[i + 1] == 0x20)
                {
                    //this is likely padding, just skip it.
                    continue;
                }
                
                //the space has a character directly after it, as long as the input has been provided properly,
                //this is probably just a space, and not padding.
            }

            finalString += Encoding.UTF8.GetString(new [] {b});
        }

        return finalString;
    }
}