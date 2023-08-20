// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Dns.Resolver.Blocklist;

public sealed class Blocklist
{
    public string Ip { get; set; } = "";

    public string[] Lists { get; set; } = Array.Empty<string>();

    public Blocklist()
    {
        
    }

    public Blocklist(string ip, string[] lists)
    {
        this.Ip = ip;
        this.Lists = lists;
    }
}
