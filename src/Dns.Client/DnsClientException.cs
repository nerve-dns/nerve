// SPDX-FileCopyrightText: 2023 - 2026 varelen and nerve contributors
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Dns.Client;

public sealed class DnsClientException : Exception
{
    public DnsClientException()
    {
    }

    public DnsClientException(string? message)
        : base(message)
    {
    }

    public DnsClientException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
