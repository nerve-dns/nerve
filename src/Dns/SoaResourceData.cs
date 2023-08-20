// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.Buffers.Binary;

namespace Nerve.Dns;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc1035#section-3.3.13
/// </summary>
public sealed class SoaResourceData : ResourceData
{
    public DomainName MName { get; set; } = new DomainName();
    public DomainName RName { get; set; } = new DomainName();
    public uint Serial { get; set; }
    public int Refresh { get; set; }
    public int Retry { get; set; }
    public int Expire { get; set; }
    public uint Minimum { get; set; }

    public override void Serialize(Span<byte> bytes, ref ushort index, Dictionary<string, ushort> domainNameOffsetCache)
    {
        this.MName.Serialize(bytes, ref index, domainNameOffsetCache);
        this.RName.Serialize(bytes, ref index, domainNameOffsetCache);
        BinaryPrimitives.WriteUInt32BigEndian(bytes.Slice(index, 4), this.Serial);
        index += 4;
        BinaryPrimitives.WriteInt32BigEndian(bytes.Slice(index, 4), this.Refresh);
        index += 4;
        BinaryPrimitives.WriteInt32BigEndian(bytes.Slice(index, 4), this.Retry);
        index += 4;
        BinaryPrimitives.WriteInt32BigEndian(bytes.Slice(index, 4), this.Expire);
        index += 4;
        BinaryPrimitives.WriteUInt32BigEndian(bytes.Slice(index, 4), this.Minimum);
        index += 4;
    }

    public override void Deserialize(ReadOnlySpan<byte> bytes, ref ushort offset)
    {
        this.MName.Deserialize(bytes, ref offset);
        this.RName.Deserialize(bytes, ref offset);
        this.Serial = BinaryPrimitives.ReadUInt32BigEndian(bytes.Slice(offset, 4));
        offset += 4;
        this.Refresh = BinaryPrimitives.ReadInt32BigEndian(bytes.Slice(offset, 4));
        offset += 4;
        this.Retry = BinaryPrimitives.ReadInt32BigEndian(bytes.Slice(offset, 4));
        offset += 4;
        this.Expire = BinaryPrimitives.ReadInt32BigEndian(bytes.Slice(offset, 4));
        offset += 4;
        this.Minimum = BinaryPrimitives.ReadUInt32BigEndian(bytes.Slice(offset, 4));
        offset += 4;
    }

    public override string ToString()
        => $"{this.MName} {this.RName} {this.Serial} {this.Refresh} {this.Retry} {this.Expire} {this.Minimum}";
}
