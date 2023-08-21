// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Dns;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc1035#section-3.2.2
/// </summary>
public enum Type : ushort
{
    /// <summary>
    /// IPv4 host address.
    /// </summary>
    A = 0x0001,
    /// <summary>
    /// Authoritative name server.
    /// </summary>
    Ns = 0x0002,
    /// <summary>
    /// Canonical name for an alias.
    /// </summary>
    CName = 0x0005,
    /// <summary>
    /// Marks the start of a zone of authority.
    /// </summary>
    Soa = 0x0006,
    /// <summary>
    /// Domain name pointer.
    /// </summary>
    Ptr = 0x000C,
    /// <summary>
    /// Mail exchange.
    /// </summary>
    Mx = 0x000F,
    /// <summary>
    /// Text strings.
    /// </summary>
    Txt = 0x0010,
    /// <summary>
    /// IPv6 host address.
    /// </summary>
    Aaaa = 0x001C,
    /// <summary>
    /// Server Selection.
    /// </summary>
    Srv = 0x0021
}
