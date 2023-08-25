// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using Nerve.Dns.Resolver.Blocklist;

namespace Nerve.Service;

public class NerveOptions
{
    public string Ip { get; set; } = "127.0.0.1";
    public ushort Port { get; set; } = 53;
    public ForwarderMode ForwarderMode { get; set; } = ForwarderMode.Https;
    public string[] Forwarders { get; set; } = Array.Empty<string>();
    public List<Blocklist> Blocklists { get; set; } = new List<Blocklist>();
    public List<Blocklist> Allowlists { get; set; } = new List<Blocklist>();
}
