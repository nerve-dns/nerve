// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Dns.Resolver.Blocklist;

public sealed class Blocklist
{
    public string Ip { get; set; } = "";

    public List<string> Lists { get; set; } = [];

    public Blocklist()
    {
        
    }

    public Blocklist(string ip, List<string> lists)
    {
        this.Ip = ip;
        this.Lists = lists;
    }
}
