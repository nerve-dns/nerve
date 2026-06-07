// SPDX-FileCopyrightText: 2023 - 2026 varelen and nerve contributors
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Dns;

public abstract class ResourceData : INetworkSerializable
{
    public abstract void Serialize(Span<byte> bytes, ref ushort index, Dictionary<string, ushort> domainNameOffsetCache);

    public abstract void Deserialize(ReadOnlySpan<byte> bytes, ref ushort offset);
}
