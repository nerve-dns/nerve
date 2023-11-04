// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.Net.Security;
using System.Net.Sockets;

namespace Nerve.Dns.Client.Tls;

public class TlsSocket
{
    public Socket Socket { get; init; } = null!;
    public SslStream SslStream  { get; set; } = null!;
}
