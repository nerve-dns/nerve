// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Dns.Resolver;

public enum Status : byte
{
    Forwarded = 0,
    Cached = 1,
    Blocked = 2
}
