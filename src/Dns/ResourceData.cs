// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Dns;

public abstract class ResourceData : INetworkSerializable
{
    public abstract ushort Length { get; }
    
    public abstract void Serialize(Span<byte> bytes, ref ushort index);

    public abstract void Deserialize(ReadOnlySpan<byte> bytes, ref ushort offset);
}
