// SPDX-FileCopyrightText: 2023 - 2026 varelen and nerve contributors
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.Net;

namespace Nerve.Dns.Tests.Unit;

public class AaaaResourceDataTests
{
    private const ushort ByteLength = 16;

    [Fact]
    public void Serialize()
    {
        // Arrange
        byte[] bytes = new byte[ByteLength];
        ushort offset = 0;

        var aaaaResourceData = new AaaaResourceData
        {
            Address = IPAddress.Parse("fd03:eca5:4f26:a123:0000:0000:0000:0000")
        };

        // Act
        aaaaResourceData.Serialize(bytes, ref offset, new Dictionary<string, ushort>());

        // Assert
        Assert.Equal(ByteLength, offset);
        Assert.Equivalent(new byte[] { 253, 3, 236, 165, 79, 38, 161, 35, 0, 0, 0, 0, 0, 0, 0, 0 }, bytes);
    }

    [Fact]
    public void Deserialize()
    {
        // Arrange
        byte[] bytes = { 253, 198, 131, 175, 126, 36, 241, 35, 0, 0, 0, 0, 0, 0, 0, 0 };
        ushort index = 0;

        var aaaaResourceData = new AaaaResourceData();

        // Act
        aaaaResourceData.Deserialize(bytes, ref index);

        // Assert
        Assert.Equal(ByteLength, index);
        Assert.Equal(IPAddress.Parse("fdc6:83af:7e24:f123:0000:0000:0000:0000"), aaaaResourceData.Address);
    }
}
