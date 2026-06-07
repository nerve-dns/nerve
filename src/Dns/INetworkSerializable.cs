// SPDX-FileCopyrightText: 2023 - 2026 varelen and nerve contributors
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Dns;

public interface INetworkSerializable
{
    void Serialize(Span<byte> bytes, ref ushort index, Dictionary<string, ushort> domainNameOffsetCache);
    void Deserialize(ReadOnlySpan<byte> bytes, ref ushort offset);
}
