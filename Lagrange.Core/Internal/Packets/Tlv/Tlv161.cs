using Lagrange.Core.Utility.Binary;
using Lagrange.Core.Utility.Binary.Tlv;
using Lagrange.Core.Utility.Binary.Tlv.Attributes;

namespace Lagrange.Core.Internal.Packets.Tlv;

[Tlv(0x161)]
internal class Tlv161 : TlvBody
{
    [BinaryProperty] public ushort u1 { get; set; }
}