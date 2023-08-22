// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Dns.Client;

public interface IDnsClient
{
    /// <summary>
    /// Resolves the given question and returns the response <see cref="Message"/>. 
    /// </summary>
    /// <param name="question">The question.</param>
    /// <param name="cancellationToken">The optional cancellation token.</param>
    /// <returns>The response <see cref="Message"/>.</returns>
    Task<Message> ResolveAsync(Question question, CancellationToken cancellationToken = default);
}
