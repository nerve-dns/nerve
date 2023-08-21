// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Dns;

/// <summary>
/// Fallback resource data for resource records that are currently not supported at the data structure level.
/// </summary>
public sealed class UnknownResourceData : ResourceData
{
    /// <summary>
    /// The data of the resource record.
    /// </summary>
    public byte[] Data { get; set; } = Array.Empty<byte>();
    
    public override void Serialize(Span<byte> bytes, ref ushort index, Dictionary<string, ushort> domainNameOffsetCache)
    {
        this.Data.CopyTo(bytes[index..]);
        index += (ushort)this.Data.Length;
    }

    public override void Deserialize(ReadOnlySpan<byte> bytes, ref ushort offset)
    {
        this.Data = new byte[bytes.Length];
        bytes.CopyTo(this.Data);
        offset += (ushort)bytes.Length;
    }

    public override string ToString()
        => $"Data Length: {this.Data.Length}";
}
