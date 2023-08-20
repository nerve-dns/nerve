// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.Net;

namespace Nerve.Dns;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc1035#section-3.4.1
/// </summary>
public sealed class AResourceData : ResourceData
{
    private const ushort Length = 4;

    /// <summary>
    /// A 32 bit IPv4 Internet address.
    /// </summary>
    public IPAddress Address { get; set; } = IPAddress.Any;

    public override ushort Length => 4;

    public override void Serialize(Span<byte> bytes, ref ushort index)
    {
        this.Address.TryWriteBytes(bytes.Slice(index, Length), out int written);
        index += (ushort)written;
    }

    public override void Deserialize(ReadOnlySpan<byte> bytes, ref ushort offset)
    {
        this.Address = new IPAddress(bytes.Slice(offset, Length));
        offset += Length;
    }

    private bool Equals(AResourceData other)
        => this.Address.Equals(other.Address);

    public override bool Equals(object? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }
        
        return other.GetType() == this.GetType() && this.Equals((AResourceData)other);
    }

    public override int GetHashCode()
        => this.Address.GetHashCode();

    public override string ToString()
        => this.Address.ToString();
}
