// SPDX-FileCopyrightText: 2023 - 2026 varelen and nerve contributors
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.Buffers.Binary;
using System.Text;

namespace Nerve.Dns;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc1035#section-4.1.4
/// </summary>
public sealed class DomainName : INetworkSerializable
{
    /// <summary>
    /// The maximum allowed length of one label.
    /// </summary>
    public const byte MaxLabelLength = 63;

    /// <summary>
    /// The maximum allowed total length of a domain name.
    /// </summary>
    public const byte MaxTotalDomainNameLength = 255;

    /// <summary>
    /// The magic byte indicating a compressed label.
    /// </summary>
    public const byte CompressedMagicByte = 0xC0;

    /// <summary>
    /// The magic unsigned short indicating a compressed label.
    /// </summary>
    public const ushort CompressedMagicUShort = 0xC000;

    /// <summary>
    /// The terminating byte indicating the end of a label.
    /// </summary>
    public const byte TerminatingByte = 0x0;

    /// <summary>
    /// The separator used to split/join the labels of a domain name.
    /// </summary>
    public const char Separator = '.';

    /// <summary>
    /// The labels representing the domain name.
    /// </summary>
    public string[] Labels { get; private set; } = [];

    public DomainName()
    {
    }

    public DomainName(string name)
        => this.Labels = name.Split(Separator);

    public void Serialize(Span<byte> bytes, ref ushort index, Dictionary<string, ushort> domainNameOffsetCache)
    {
        int totalDomainNameLength = 0;

        for (int i = 0; i < this.Labels.Length; i++)
        {
            // Compress already serialized domain name parts (eg., reuse google.com in youtube-ui.l.google.com if serialized earlier)
            string domainNameSlice = this.ToString(i);

            if (domainNameOffsetCache.TryGetValue(domainNameSlice, out ushort domainNameOffset))
            {
                BinaryPrimitives.WriteUInt16BigEndian(bytes.Slice(index, 2), (ushort)(CompressedMagicUShort | domainNameOffset));
                index += 2;
                return;
            }

            domainNameOffsetCache.Add(domainNameSlice, index);

            string label = this.Labels[i];

            if (label.Length > MaxLabelLength)
            {
                throw new ArgumentOutOfRangeException(nameof(bytes), $"Label length '{label.Length}' exceeds maximum of '{MaxLabelLength}'");
            }

            bytes[index++] = (byte)label.Length;
            foreach (char character in label)
            {
                bytes[index++] = (byte)character;
            }

            totalDomainNameLength += label.Length;
        }

        if (totalDomainNameLength > MaxTotalDomainNameLength)
        {
            throw new ArgumentOutOfRangeException(nameof(bytes), $"Total domain name length '{totalDomainNameLength}' exceeds maximum of '{MaxTotalDomainNameLength}'");
        }

        bytes[index++] = TerminatingByte;
    }

    public void Deserialize(ReadOnlySpan<byte> bytes, ref ushort offset)
    {
        var labels = new List<string>();
        short compressedOffset = -1;
        byte labelLength;
        int totalDomainNameLength = 0;

        while ((labelLength = bytes[offset++]) != 0)
        {
            // "Decompress" label by jumping to the offset of the previous serialized label
            if ((labelLength & CompressedMagicByte) == CompressedMagicByte)
            {
                if (compressedOffset == -1)
                {
                    compressedOffset = (short)offset;
                }

                byte compressedMagicBitsRemoved = (byte)(labelLength ^ CompressedMagicByte);
                ushort offsetShifted = (ushort)(compressedMagicBitsRemoved << 8);

                offset = (ushort)(offsetShifted | bytes[offset]);
                labelLength = bytes[offset];
                offset++;
            }

            if (labelLength > MaxLabelLength)
            {
                throw new ArgumentOutOfRangeException(nameof(bytes), $"Label length '{labelLength}' exceeds maximum of '{MaxLabelLength}'");
            }

            string label = Encoding.ASCII.GetString(bytes.Slice(offset, labelLength));
            offset += labelLength;
            labels.Add(label);

            totalDomainNameLength += labelLength;
        }

        if (totalDomainNameLength > MaxTotalDomainNameLength)
        {
            throw new ArgumentOutOfRangeException(nameof(bytes), $"Total domain name length '{totalDomainNameLength}' exceeds maximum of '{MaxTotalDomainNameLength}'");
        }

        if (compressedOffset != -1)
        {
            offset = (ushort)(compressedOffset + 1);
        }

        this.Labels = labels.ToArray();
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
        
        return other.GetType() == this.GetType() && this.Equals((DomainName)other);
    }

    public override int GetHashCode()
    {
        var hashCode = default(HashCode);

        foreach (var label in this.Labels)
        {
            hashCode.Add(label);
        }

        return hashCode.ToHashCode();
    }

    public override string ToString()
        => this.ToString(startLabelIndex: 0);

    public string ToString(int startLabelIndex)
        => string.Join(Separator, this.Labels, startLabelIndex, this.Labels.Length - startLabelIndex);

    public static implicit operator string(DomainName domainName)
        => domainName.ToString();
}
