// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.Text;

namespace Nerve.Dns;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc1035#section-4.1.4
///  
/// TODO: This is currently not a complete implementation because compression in serialize is missing among other things.
/// </summary>
public sealed class DomainName : INetworkSerializable
{
    /// <summary>
    /// The magic byte indicating a compressed label.
    /// </summary>
    public const byte CompressedMagicByte = 0xC0;

    /// <summary>
    /// The labels representing the domain name.
    /// </summary>
    public List<string> Labels { get; } = new();

    public DomainName()
    {
    }

    public DomainName(string name)
    {
        this.Labels.AddRange(name.Split('.'));
    }

    public void Serialize(Span<byte> bytes, ref ushort index)
    {
        foreach (string label in this.Labels)
        {
            bytes[index++] = (byte)label.Length;
            foreach (char character in label)
            {
                bytes[index++] = (byte)character;
            }
        }

        bytes[index++] = 0x0;
    }

    public void Deserialize(ReadOnlySpan<byte> bytes, ref ushort offset)
    {
        short compressedOffset = -1;
        byte labelLength;
        while ((labelLength = bytes[offset++]) != 0)
        {
            if (labelLength == CompressedMagicByte)
            {
                if (compressedOffset == -1)
                {
                    compressedOffset = (short)offset;
                }

                offset = bytes[offset];
                labelLength = bytes[offset];
                offset++;
            }

            string label = Encoding.ASCII.GetString(bytes.Slice(offset, labelLength));
            offset += labelLength;
            Labels.Add(label);
        }

        if (compressedOffset != -1)
        {
            offset = (ushort)(compressedOffset + 1);
        }
    }

    private bool Equals(DomainName other)
        => this.Labels.SequenceEqual(other.Labels);

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
        
        return other.GetType() == this.GetType() && Equals((DomainName)other);
    }

    public override int GetHashCode()
        => this.Labels.GetHashCode();

    public override string ToString()
        => string.Join('.', this.Labels);

    public static implicit operator string(DomainName domainName)
        => domainName.ToString();
}
