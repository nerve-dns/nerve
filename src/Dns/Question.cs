// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.Buffers.Binary;

namespace Nerve.Dns;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc1035#section-4.1.2
/// </summary>
public class Question : INetworkSerializable
{
    public DomainName Name { get; set; } = new();
    public Type Type { get; set; } = Type.A;
    public Class Class { get; set; } = Class.In;

    public Question()
    {
    }

    public Question(
        DomainName name,
        Type type,
        Class @class)
    {
        this.Name = name;
        this.Type = type;
        this.Class = @class;
    }

    public void Serialize(Span<byte> bytes, ref ushort index)
    {
        this.Name.Serialize(bytes, ref index);
        BinaryPrimitives.WriteUInt16BigEndian(bytes[index..2], (ushort)this.Type);
        index += 2;
        BinaryPrimitives.WriteUInt16BigEndian(bytes[index..2], (ushort)this.Class);
        index += 2;
    }

    public void Deserialize(ReadOnlySpan<byte> bytes, ref ushort offset)
    {
        this.Name = new DomainName();
        this.Name.Deserialize(bytes, ref offset);
        this.Type = (Type)BinaryPrimitives.ReadUInt16BigEndian(bytes.Slice(offset, 2));
        this.Class = (Class)BinaryPrimitives.ReadUInt16BigEndian(bytes.Slice(offset + 2, 2));
        offset += 4;
    }

    private bool Equals(Question other)
        => this.Name.Equals(other.Name) && this.Type == other.Type && this.Class == other.Class;

    public override bool Equals(object? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }
        
        return other.GetType() == this.GetType() && this.Equals((Question)other);
    }

    public override int GetHashCode()
        => HashCode.Combine(this.Name, (ushort)this.Type, (ushort)this.Class);

    public override string ToString()
        => $"{nameof(this.Name)}: {(string)this.Name}, {nameof(this.Type)}: {this.Type}, {nameof(this.Class)}: {this.Class}";
}
