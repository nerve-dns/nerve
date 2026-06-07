// SPDX-FileCopyrightText: 2023 - 2026 varelen and nerve contributors
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Dns.Client.Tests.Unit;

public class RoundRobinUriProviderTests
{
    [Fact]
    public void Get_ListOfUris_ReturnsUrisInRoundRobinFashion()
    {
        // Arrange
        var uris = new[] { new Uri("https://example.com/one"), new Uri("https://example.com/two") };
        var roundRobinUriProvider = new RoundRobinUriProvider(uris);
        var actualUris = new List<Uri>();

        // Act
        for (int i = 0; i < 5; i++)
        {
            actualUris.Add(roundRobinUriProvider.Get());
        }

        // Assert
        Assert.Equivalent(new List<Uri>
        {
            new("https://example.com/one"),
            new("https://example.com/two"),
            new("https://example.com/one"),
            new("https://example.com/two"),
            new("https://example.com/one"),
        }, actualUris);
    }
}
