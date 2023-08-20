// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Dns.Tests.Unit;

public class FlagsTests
{
    [Fact]
    public void Serialize_ValidFlags_WritesBytesCorrect()
    {
        // Arrange
        byte[] buffer = new byte[2];
        ushort index = 0;
        var flags = new Flags
        {
            OpCode = OpCode.Query,
            QueryResponse = true,
            AuthoritativeAnswer = false,
            RecursionAvailable = false,
            RecursionDesired = true,
            ResponseCode = ResponseCode.NxDomain,
            Truncation = false
        };

        // Act
        flags.Serialize(buffer, ref index, new Dictionary<string, ushort>());
        
        // Assert
        buffer.Should().BeEquivalentTo(new byte[] { 129, 3 });
    }

    [Fact]
    public void Deserialize_ValidFlags_ReadsBytesCorrect()
    {
        // Arrange
        byte[] buffer = new byte[] { 129, 3 };
        ushort offset = 0;
        var flags = new Flags();

        // Act
        flags.Deserialize(buffer, ref offset);
        
        // Assert
        flags.Should().BeEquivalentTo(new Flags
        {
            OpCode = OpCode.Query,
            QueryResponse = true,
            AuthoritativeAnswer = false,
            RecursionAvailable = false,
            RecursionDesired = true,
            ResponseCode = ResponseCode.NxDomain,
            Truncation = false
        });
    }
}
