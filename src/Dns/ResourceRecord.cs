// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.Buffers.Binary;

namespace Nerve.Dns;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc1035#section-4.1.3
/// </summary>
public sealed class ResourceRecord : INetworkSerializable
{
    public DomainName Name { get; set; } = new();
    public Type Type { get; set; }
    public Class Class { get; set; }
    public uint Ttl { get; set; }
    public ushort ResourceDataLength { get; set; }
    public ResourceData? ResourceData { get; set; }

    public void Serialize(Span<byte> bytes, ref ushort index, Dictionary<string, ushort> domainNameOffsetCache)
    {
        this.Name.Serialize(bytes, ref index, domainNameOffsetCache);
        BinaryPrimitives.WriteUInt16BigEndian(bytes.Slice(index, 2), (ushort)this.Type);
        BinaryPrimitives.WriteUInt16BigEndian(bytes.Slice(index + 2, 2), (ushort)this.Class);
        BinaryPrimitives.WriteUInt32BigEndian(bytes.Slice(index + 4, 4), this.Ttl);
        index += 8;

        if (this.ResourceData is not null)
        {
            ushort resourceDataLengthOffset = index;
            index += 2;

            ushort beforeResourceDataOffset = index;
            this.ResourceData.Serialize(bytes, ref index, domainNameOffsetCache);
            
            ushort resourceDataLength = (ushort)(index - beforeResourceDataOffset);
            BinaryPrimitives.WriteUInt16BigEndian(bytes.Slice(resourceDataLengthOffset, 2), resourceDataLength);
        }
        else
        {
            bytes[index++] = 0;
            bytes[index++] = 0;
        }
    }

    public void Deserialize(ReadOnlySpan<byte> bytes, ref ushort offset)
    {
        this.Name.Deserialize(bytes, ref offset);
        this.Type = (Type)BinaryPrimitives.ReadUInt16BigEndian(bytes.Slice(offset, 2));
        this.Class = (Class)BinaryPrimitives.ReadUInt16BigEndian(bytes.Slice(offset + 2, 2));
        this.Ttl = BinaryPrimitives.ReadUInt32BigEndian(bytes.Slice(offset + 4, 4));
        this.ResourceDataLength = BinaryPrimitives.ReadUInt16BigEndian(bytes.Slice(offset + 8, 2));
        offset += 10;

        switch (this.Class)
        {
            case Class.In when this.Type == Type.A:
            {
                var aResourceData = new AResourceData();
                aResourceData.Deserialize(bytes, ref offset);
                this.ResourceData = aResourceData;
                break;
            }
            case Class.In when this.Type == Type.Aaaa:
            {
                var aaaaResourceData = new AaaaResourceData();
                aaaaResourceData.Deserialize(bytes, ref offset);
                this.ResourceData = aaaaResourceData;
                break;
            }
            case Class.In when this.Type == Type.Ns:
            {
                var nsResourceData = new NsResourceData();
                nsResourceData.Deserialize(bytes, ref offset);
                this.ResourceData = nsResourceData;
                break;
            }
            case Class.In when this.Type == Type.CName:
            {
                var cNameResourceData = new CNameResourceData();
                cNameResourceData.Deserialize(bytes, ref offset);
                this.ResourceData = cNameResourceData;
                break;
            }
            default:
                // TODO: Throw domain exception or something more performant?
                // A server/client needs to handle this properly eg., return ResponseCode.NotImp
                Console.WriteLine("Unsupported class " + Class + " and type " + Type);
                break;
        }
    }

    private bool Equals(ResourceRecord other)
        => this.Name.Equals(other.Name)
            && this.Type == other.Type
            && this.Class == other.Class
            && this.Ttl == other.Ttl
            && this.ResourceDataLength == other.ResourceDataLength
            && Equals(this.ResourceData, other.ResourceData);

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

        return other.GetType() == this.GetType() && this.Equals((ResourceRecord)other);
    }

    public override int GetHashCode()
        => HashCode.Combine(this.Name, (int)this.Type, (int)this.Class, this.Ttl, this.ResourceDataLength, this.ResourceData);

    public override string ToString()
        => $"{nameof(this.Name)}: {this.Name}, {nameof(this.Type)}: {this.Type}, {nameof(this.Class)}: {this.Class}, {nameof(this.Ttl)}: {this.Ttl}, {nameof(this.ResourceDataLength)}: {this.ResourceDataLength}, {nameof(this.ResourceData)}: {this.ResourceData}";
}
