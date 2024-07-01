using Lagrange.Core.Utility.Binary;
using Lagrange.Core.Utility.Binary.Tlv;
using Lagrange.Core.Utility.Binary.Tlv.Attributes;
#pragma warning disable CS8618

namespace Lagrange.Core.Internal.Packets.Tlv;

[Tlv(0x10C, true)]
internal class Tlv10C : TlvBody
{
    [BinaryProperty(Prefix.None)] public byte[] Tgtgt { get; set; }
}