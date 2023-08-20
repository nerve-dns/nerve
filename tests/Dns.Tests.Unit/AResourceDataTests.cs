// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.Net;

namespace Nerve.Dns.Tests.Unit;

public class AResourceDataTests
{
    private const ushort ByteLength = 4;

    [Fact]
    public void Serialize()
    {
        // Arrange
        byte[] bytes = new byte[ByteLength];
        ushort index = 0;

        var aResourceData = new AResourceData
        {
            Address = IPAddress.Parse("10.1.2.3")
        };

        // Act
        aResourceData.Serialize(bytes, ref index, new Dictionary<string, ushort>());

        // Assert
        index.Should().Be(ByteLength);
        bytes.Should().BeEquivalentTo(new byte[] { 10, 1, 2, 3 });
    }

    [Fact]
    public void Deserialize()
    {
        // Arrange
        byte[] bytes = new byte[] { 10, 0, 0, 1 };
        ushort offset = 0;
        var aResourceData = new AResourceData();

        // Act
        aResourceData.Deserialize(bytes, ref offset);

        // Assert
        offset.Should().Be(ByteLength);
        aResourceData.Address.Should().Be(IPAddress.Parse("10.0.0.1"));
    }
}
