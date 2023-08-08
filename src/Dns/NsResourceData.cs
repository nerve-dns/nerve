// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Dns;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc1035#section-3.3.11
/// </summary>
public sealed class NsResourceData : ResourceData
{
    /// <summary>
    /// A domain-name which specifies a host which should be authoritative for the specified class and domain.
    /// </summary>
    public DomainName Name { get; set; } = new();

    public override ushort Length => (ushort)(this.Name.ToString().Length + 1);

    public override void Serialize(Span<byte> bytes, ref ushort index)
        => this.Name.Serialize(bytes, ref index);

    public override void Deserialize(ReadOnlySpan<byte> bytes, ref ushort offset)
        => this.Name.Deserialize(bytes, ref offset);

    public override string ToString()
        => $"{nameof(this.Name)}: {this.Name}";
}
