// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Dns;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc1035#section-3.3.1
/// </summary>
public class CNameResourceData : ResourceData
{
    /// <summary>
    /// A domain-name which specifies the canonical or primary name for the owner. The owner name is an alias.
    /// </summary>
    public DomainName Name { get; set; } = new();



    public override void Serialize(Span<byte> bytes, ref ushort index, Dictionary<string, ushort> domainNameOffsetCache)
        => this.Name.Serialize(bytes, ref index, domainNameOffsetCache);

    public override void Deserialize(ReadOnlySpan<byte> bytes, ref ushort offset)
        => this.Name.Deserialize(bytes, ref offset);

    public override string ToString()
        => $"{nameof(this.Name)}: {this.Name}";
}
