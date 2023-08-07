// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Dns.Tests.Unit;

public class DomainNameTests
{
    private static readonly byte[] ExampleDotComDomainNameBytes =
    {
        7, 101, 120, 97, 109, 112, 108, 101, 3, 99, 111, 109, 0
    };

    // The zero at the end is the index to jump to which comes after the compressed magic byte
    private static readonly byte[] ExampleDotComDomainNameBytesWithCompression =
    {
        7, 101, 120, 97, 109, 112, 108, 101, 3, 99, 111, 109, 0, DomainName.CompressedMagicByte, 0
    };

    [Fact]
    public void Equals_TwoInstancesWithSameDomain_ReturnsTrue()
    {
        // Arrange
        var domainName = new DomainName("example.com");
        var domainName2 = new DomainName("example.com");
        
        // Act
        bool equals = domainName.Equals(domainName2);
        
        // Assert
        equals.Should().BeTrue();
    }

    [Fact]
    public void Equals_TwoInstancesWithDifferentDomain_ReturnsFalse()
    {
        // Arrange
        var domainName = new DomainName("example.com");
        var domainName2 = new DomainName("example.de");
        
        // Act
        bool equals = domainName.Equals(domainName2);
        
        // Assert
        equals.Should().BeFalse();
    }

    [Fact]
    public void ToString_ValidDomainName_ReturnsCorrectString()
    {
        // Arrange
        var domainName = new DomainName("www.example-domain.com");
        
        // Act
        string text = domainName.ToString();
        
        // Assert
        text.Should().Be("www.example-domain.com");
    }

    [Fact]
    public void Serialize_WithoutCompression_ReturnsExpectedBytes()
    {
        // Arrange
        byte[] buffer = new byte[13];
        ushort index = 0;
        var domainName = new DomainName("example.com");
        
        // Act
        domainName.Serialize(buffer, ref index);
        
        // Assert
        index.Should().Be(13);
        buffer.Should().BeEquivalentTo(ExampleDotComDomainNameBytes);
    }

    [Fact]
    public void Deserialize_WithoutCompression_ReturnsExpectedDomainName()
    {
        // Arrange
        ushort offset = 0;
        var domainName = new DomainName();
        
        // Act
        domainName.Deserialize(ExampleDotComDomainNameBytes, ref offset);
        
        // Assert
        offset.Should().Be(13);
        domainName.Labels.Count.Should().Be(2);
        domainName.ToString().Should().Be("example.com");
    }
    
    [Fact]
    public void Deserialize_WithCompression_ReturnsExpectedDomainName()
    {
        // Arrange
        ushort offset = 13;
        var domainName = new DomainName();
        
        // Act
        domainName.Deserialize(ExampleDotComDomainNameBytesWithCompression, ref offset);
        
        // Assert
        offset.Should().Be(15);
        domainName.Labels.Count.Should().Be(2);
        domainName.ToString().Should().Be("example.com");
    }
}