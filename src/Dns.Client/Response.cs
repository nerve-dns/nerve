// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Dns.Client;

public class DnsResponse
{
    /// <summary>
    /// The <see cref="Dns.ResponseCode">ResponseCode</see> of this response.
    /// </summary>
    public ResponseCode ResponseCode { get; set; }
}
