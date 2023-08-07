// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Dns;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc1035#section-4.1.1
/// </summary>
public enum ResponseCode : byte
{
    /// <summary>
    /// No error condition.
    /// </summary>
    NoError = 0,
    /// <summary>
    /// Format error - The name server was unable to interpret the query.
    /// </summary>
    FormErr = 1,
    /// <summary>
    /// Server failure - The name server was unable to process this query due to a problem with the name server.
    /// </summary>
    ServFail = 2,
    /// <summary>
    /// Name Error - Meaningful only for responses from an authoritative name server,
    /// this code signifies that the domain name referenced in the query does not exist.
    /// </summary>
    NxDomain = 3,
    /// <summary>
    /// Not Implemented - The name server does not support the requested kind of query.
    /// </summary>
    NotImp = 4,
    /// <summary>
    /// Refused - The name server refuses to perform the specified operation for policy reasons. For example, a name
    /// server may not wish to provide the information to the particular requester, or a name server may not wish to perform
    /// a particular operation (e.g., zone transfer) for particular data.
    /// </summary>
    Refused = 5
}
