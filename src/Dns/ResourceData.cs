// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Dns;

public abstract class ResourceData : INetworkSerializable
{

    public abstract void Serialize(Span<byte> bytes, ref ushort index, Dictionary<string, ushort> domainNameOffsetCache);

    public abstract void Deserialize(ReadOnlySpan<byte> bytes, ref ushort offset);
}
