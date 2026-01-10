// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Dns.Client.Tls;

public sealed class ConnectionClosedException : IOException
{
    public ConnectionClosedException()
    {
    }

    public ConnectionClosedException(string? message)
        : base(message)
    {
    }

    public ConnectionClosedException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
