// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Dns;

public interface INetworkSerializable
{
    void Serialize(Span<byte> bytes, ref ushort index);
    void Deserialize(ReadOnlySpan<byte> bytes, ref ushort offset);
}
