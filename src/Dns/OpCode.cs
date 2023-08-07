// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Dns;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc1035#section-4.1.1
/// </summary>
public enum OpCode : byte
{
    /// <summary>
    /// Standard query.
    /// </summary>
    Query = 0,
    /// <summary>
    /// Inverse query.
    /// </summary>
    InverseQuery = 1,
    /// <summary>
    /// Server status request.
    /// </summary>
    Status = 2
}
