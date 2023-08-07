// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Dns;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc1035#section-3.2.4
/// </summary>
public enum Class : ushort
{
    /// <summary>
    /// Internet.
    /// </summary>
    In = 1,
    /// <summary>
    /// CSNET (obsolete).
    /// </summary>
    Cs = 2,
    /// <summary>
    /// CHAOS.
    /// </summary>
    Ch = 3,
    /// <summary>
    /// Hesiod.
    /// </summary>
    Hs = 4
}
