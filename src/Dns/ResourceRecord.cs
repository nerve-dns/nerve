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
    /// <summary>
    /// A domain name to which this resource record pertains.
    /// </summary>
    public DomainName Name { get; set; } = new();

    /// <summary>
    /// This field specifies the meaning of the data in the RDATA field.
    /// </summary>
    public Type Type { get; set; }

    /// <summary>
    /// The class of the data in the RDATA field.
    /// </summary>
    public Class Class { get; set; }

    /// <summary>
    /// Specifies the time interval (in seconds) that the resource record may be 
    /// cached before it should be discarded. Zero values are 
    /// interpreted to mean that the RR can only be used for the 
    /// transaction in progress, and should not be cached.
    /// </summary>
    public uint Ttl { get; set; }

    /// <summary>
    /// The length of the RDATA field (only set after deserialization or serialization).
    /// </summary>
    public ushort ResourceDataLength { get; set; }

    /// <summary>
    /// The <see cref="ResourceData"/> describing the resource.
    /// The type varies based on the TYPE and CLASS of the resource record.
    /// For example, if the TYPE is A and the CLASS is IN, the <see cref="ResourceData"/> will
    /// be of type <see cref="AResourceData"/>.
    /// </summary>
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
            
            this.ResourceDataLength = (ushort)(index - beforeResourceDataOffset);
            BinaryPrimitives.WriteUInt16BigEndian(bytes.Slice(resourceDataLengthOffset, 2), this.ResourceDataLength);
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
            case Class.In when this.Type == Type.Soa:
            {
                var soaResourceData = new SoaResourceData();
                soaResourceData.Deserialize(bytes, ref offset);
                this.ResourceData = soaResourceData;
                break;
            }
            case Class.In when this.Type == Type.Mx:
            {
                var mxResourceData = new MxResourceData();
                mxResourceData.Deserialize(bytes, ref offset);
                this.ResourceData = mxResourceData;
                break;
            }
            case Class.In when this.Type == Type.Ptr:
            {
                var ptrResourceData = new PtrResourceData();
                ptrResourceData.Deserialize(bytes, ref offset);
                this.ResourceData = ptrResourceData;
                break;
            }
            case Class.In when this.Type == Type.Srv:
            {
                var srvResourceDate = new SrvResourceData();
                srvResourceDate.Deserialize(bytes, ref offset);
                this.ResourceData = srvResourceDate;
                break;
            }
            default:
            {
                var unknownResourceData = new UnknownResourceData();
                unknownResourceData.Deserialize(bytes.Slice(offset, this.ResourceDataLength), ref offset);
                this.ResourceData = unknownResourceData;
                break;
            }
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
        => $"{nameof(this.Name)}: {this.Name}, {nameof(this.Type)}: {this.Type}, {nameof(this.Class)}: {this.Class}, {nameof(this.Ttl)}: {this.Ttl}, {nameof(this.ResourceDataLength)}: {this.ResourceDataLength}, {nameof(this.ResourceData)}({this.ResourceData?.GetType().Name}): {this.ResourceData}";
}
