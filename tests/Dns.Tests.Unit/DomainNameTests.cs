// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Dns.Tests.Unit;

public class DomainNameTests
{
    // example.com
    private static readonly byte[] ExampleDotComDomainNameBytes =
    {
        0x07, 0x65, 0x78, 0x61, 0x6d, 0x70, 0x6c, 0x65, 0x3, 0x63, 0x6f, 0x6d, 0x00
    };

    // The zero at the end is the index to jump to which comes after the compressed magic byte
    // example.com
    private static readonly byte[] ExampleDotComDomainNameBytesWithCompression =
    {
        0x07, 0x65, 0x78, 0x61, 0x6d, 0x70, 0x6c, 0x65, 0x3, 0x63, 0x6f, 0x6d, 0x00, DomainName.CompressedMagicByte, 0x00
    };

    // example.com and www.example.com
    private static readonly byte[] TwiceExampleDotComDomainNameBytesWithCompression =
    {
        0x07, 0x65, 0x78, 0x61, 0x6D, 0x70, 0x6C, 0x65, 0x03, 0x63, 0x6F, 0x6D, 0x00, 0x03, 0x77, 0x77, 0x77, DomainName.CompressedMagicByte, 0x00
    };

    [Fact]
    public void Separator_ReturnsDot()
    {
        // Arrange
        const char expectedSeparator = '.';
        
        // Act
        const char actualSeparator = DomainName.Separator;
        
        // Assert
        actualSeparator.Should().Be(expectedSeparator);
    }

    [Fact]
    public void GetHashCode_DifferentInstancesButSameValue_ReturnsSameHashCode()
    {
        // Arrange
        const string domain = "www.example.com";
        var domainNameFirst = new DomainName(domain);
        var domainNameSecond = new DomainName(domain);
        
        // Act
        int hashCodeFirst = domainNameFirst.GetHashCode();
        int hashCodeSecond = domainNameSecond.GetHashCode();
        
        // Assert
        hashCodeFirst.Should().Be(hashCodeSecond);
    }

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
    public void ToString_ValidDomainNameWithStartLabelIndex_ReturnsCorrectString()
    {
        // Arrange
        var domainName = new DomainName("www.example-domain.com");
        
        // Act
        string text = domainName.ToString(startLabelIndex: 1);
        
        // Assert
        text.Should().Be("example-domain.com");
    }

    [Fact]
    public void Serialize_WithoutCompression_ReturnsExpectedBytes()
    {
        // Arrange
        byte[] buffer = new byte[13];
        ushort index = 0;
        var domainName = new DomainName("example.com");
        
        // Act
        domainName.Serialize(buffer, ref index, new Dictionary<string, ushort>());
        
        // Assert
        index.Should().Be(13);
        buffer.Should().BeEquivalentTo(ExampleDotComDomainNameBytes);
    }

    [Fact]
    public void Serialize_WithCompression_ReturnsExpectedBytes()
    {
        // Arrange
        byte[] buffer = new byte[19];
        ushort index = 0;
        var domainName = new DomainName("example.com");
        var domainNameSecond = new DomainName("www.example.com");
        var domainNameOffsetCache = new Dictionary<string, ushort>();
        
        // Act
        domainName.Serialize(buffer, ref index, domainNameOffsetCache);
        domainNameSecond.Serialize(buffer, ref index, domainNameOffsetCache);
        
        // Assert
        index.Should().Be(19);
        buffer.Should().BeEquivalentTo(TwiceExampleDotComDomainNameBytesWithCompression);
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
        domainName.Labels.Length.Should().Be(2);
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
        domainName.Labels.Length.Should().Be(2);
        domainName.ToString().Should().Be("example.com");
    }
}
