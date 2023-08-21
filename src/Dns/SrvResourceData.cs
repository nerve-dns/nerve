// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.Buffers.Binary;

namespace Nerve.Dns;

/// <summary>
/// https://www.rfc-editor.org/rfc/rfc2782.html
/// </summary>
public sealed class SrvResourceData : ResourceData
{
    /// <summary>
    /// The priority of this target host.
    /// </summary>
    public ushort Priority { get; set; }

    /// <summary>
    /// The weight field which specifies a relative weight for entries with the same priority.
    /// </summary>
    public ushort Weight { get; set; }

    /// <summary>
    /// The port on this target host of this service.
    /// </summary>
    public ushort Port { get; set; }

    /// <summary>
    /// A domain-name of the target host.
    /// </summary>
    public DomainName Target { get; set; } = new();
    
    public override void Serialize(Span<byte> bytes, ref ushort index, Dictionary<string, ushort> domainNameOffsetCache)
    {
        BinaryPrimitives.WriteUInt16BigEndian(bytes.Slice(index, 2), this.Priority);
        BinaryPrimitives.WriteUInt16BigEndian(bytes.Slice(index + 2, 2), this.Weight);
        BinaryPrimitives.WriteUInt16BigEndian(bytes.Slice(index + 4, 2), this.Port);
        index += 6;

        this.Target.Serialize(bytes, ref index, domainNameOffsetCache);
    }

    public override void Deserialize(ReadOnlySpan<byte> bytes, ref ushort offset)
    {
        this.Priority = BinaryPrimitives.ReadUInt16BigEndian(bytes.Slice(offset, 2));
        this.Weight = BinaryPrimitives.ReadUInt16BigEndian(bytes.Slice(offset + 2, 2));
        this.Port = BinaryPrimitives.ReadUInt16BigEndian(bytes.Slice(offset + 4, 2));
        offset += 6;

        this.Target.Deserialize(bytes, ref offset);
    }

    public override string ToString()
        => $"{this.Priority} {this.Weight} {this.Port} {this.Target}";
}
