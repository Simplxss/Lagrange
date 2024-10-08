using Lagrange.Core.Common;
using Lagrange.Core.Utility.Binary;
using Lagrange.Core.Utility.Binary.Tlv;
using Lagrange.Core.Utility.Binary.Tlv.Attributes;

namespace Lagrange.Core.Internal.Packets.Tlv;

[Tlv(0x147)]
internal class Tlv147 : TlvBody
{
    public Tlv147(BotAppInfo appInfo)
    {
        AppId = appInfo.AppId;
        PtVersion = appInfo.PtVersion;
        ApkSignatureMd5 = appInfo.ApkSignatureMd5;
    }
    
    [BinaryProperty] public uint AppId { get; set; }
    
    [BinaryProperty(Prefix.Uint16 | Prefix.LengthOnly)] public string PtVersion { get; set; }
    
    [BinaryProperty(Prefix.Uint16 | Prefix.LengthOnly)] public byte[] ApkSignatureMd5 { get; set; }
}