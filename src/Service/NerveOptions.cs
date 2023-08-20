// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using Nerve.Dns.Resolver.Blocklist;

namespace Nerve.Service;

public class NerveOptions
{
    public string Ip { get; set; } = "127.0.0.1";
    public ushort Port { get; set; } = 53;
    public string[] Forwarders { get; set; } = new string[] { "1.1.1.1" };
    public List<Blocklist>? Blocklists { get; set; }
}
