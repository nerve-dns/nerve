// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.Buffers.Binary;

namespace Nerve.Dns;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc1035#section-3.3.9
/// </summary>
public sealed class MxResourceData : ResourceData
{
    /// <summary>
    /// A 16 bit integer which specifies the preference given to this RR among others at the same owner. Lower values are preferred.
    /// </summary>
    public short Preference { get; set; }
    
    /// <summary>
    /// A domain-name which specifies a host willing to act as a mail exchange for the owner name.
    /// </summary>
    public DomainName Exchange { get; set; } = new DomainName();
    
    public override void Serialize(Span<byte> bytes, ref ushort index, Dictionary<string, ushort> domainNameOffsetCache)
    {
        BinaryPrimitives.WriteInt16BigEndian(bytes.Slice(index, 2), this.Preference);
        index += 2;
        this.Exchange.Serialize(bytes, ref index, domainNameOffsetCache);
    }

    public override void Deserialize(ReadOnlySpan<byte> bytes, ref ushort offset)
    {
        this.Preference = BinaryPrimitives.ReadInt16BigEndian(bytes.Slice(offset, 2));
        offset += 2;
        this.Exchange.Deserialize(bytes, ref offset);
    }

    public override string ToString()
        => $"{this.Preference} {this.Exchange}";
}
