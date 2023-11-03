// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.ComponentModel.DataAnnotations;

using Nerve.Dns.Resolver.Blocklist;

namespace Nerve.Service;

public class NerveOptions
{
    public const string SectionName = "Nerve";

    public string Ip { get; set; } = "127.0.0.1";

    [Range(ushort.MinValue, ushort.MaxValue)]
    public int Port { get; set; } = 53;
    public PrivacyMode PrivacyMode { get; set; } = PrivacyMode.Everything;
    public ForwarderMode ForwarderMode { get; set; } = ForwarderMode.Https;
    public string[] Forwarders { get; set; } = Array.Empty<string>();
    public List<Blocklist> Blocklists { get; set; } = new List<Blocklist>();
    public List<Blocklist> Allowlists { get; set; } = new List<Blocklist>();
}
