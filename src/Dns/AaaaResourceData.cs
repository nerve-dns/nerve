// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.Net;

namespace Nerve.Dns;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc1886#section-2
/// </summary>
public class AaaaResourceData : ResourceData
{
    /// <summary>
    /// A 128 bit IPv6 Internet address.
    /// </summary>
    public IPAddress Address { get; set; } = IPAddress.IPv6Any;

    public override ushort Length => 16;

    public override void Serialize(Span<byte> bytes, ref ushort index)
    {
        this.Address.TryWriteBytes(bytes.Slice(index, this.Length), out int written);
        index += (ushort)written;
    }

    public override void Deserialize(ReadOnlySpan<byte> bytes, ref ushort offset)
    {
        this.Address = new IPAddress(bytes.Slice(offset, this.Length));
        offset += this.Length;
    }

    private bool Equals(AaaaResourceData other)
        => Address.Equals(other.Address);

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
        
        return other.GetType() == this.GetType() && this.Equals((AaaaResourceData)other);
    }

    public override int GetHashCode()
        => this.Address.GetHashCode();

    public override string ToString()
        => this.Address.ToString();
}
