// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.Net;

namespace Nerve.Dns.Client.Tests.Unit;

public class RoundRobinIpEndPointProviderTests
{
    [Fact]
    public void Get_ListOfIpEndPoints_ReturnsIpEndPointsInRoundRobinFashion()
    {
        // Arrange
        var ipEndPoints = new[] { new IPEndPoint(IPAddress.Parse("1.1.1.1"), 53), new IPEndPoint(IPAddress.Parse("1.0.0.1"), 53) };
        var roundRobinIpEndPointProvider = new RoundRobinIpEndPointProvider(ipEndPoints);
        var actualIpEndPoints = new List<IPEndPoint>();

        // Act
        for (int i = 0; i < 5; i++)
        {
            actualIpEndPoints.Add(roundRobinIpEndPointProvider.Get());
        }
        
        // Assert
        actualIpEndPoints.Should().BeEquivalentTo(new List<IPEndPoint>
        {
            new IPEndPoint(IPAddress.Parse("1.1.1.1"), 53),
            new IPEndPoint(IPAddress.Parse("1.0.0.1"), 53),
            new IPEndPoint(IPAddress.Parse("1.1.1.1"), 53),
            new IPEndPoint(IPAddress.Parse("1.0.0.1"), 53),
            new IPEndPoint(IPAddress.Parse("1.1.1.1"), 53)
        });
    }
}
