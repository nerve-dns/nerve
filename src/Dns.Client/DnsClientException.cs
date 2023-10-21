// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.Runtime.Serialization;

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

    public DnsClientException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

    public DnsClientException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
