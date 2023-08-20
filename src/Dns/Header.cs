// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.Buffers.Binary;

namespace Nerve.Dns;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc1035#section-4.1.1
/// </summary>
public class Header : INetworkSerializable
{
    public ushort Id { get; set; }
    public Flags Flags { get; set; } = new();
    public ushort QuestionCount { get; set; }
    public ushort AnswerCount { get; set; }
    public ushort AuthorityCount { get; set; }
    public ushort AdditionalCount { get; set; }

    public bool Query => !this.QueryResponse;
    
    public bool QueryResponse
    {
        get => this.Flags.QueryResponse;
        set => this.Flags.QueryResponse = value;
    }
    
    public OpCode OpCode
    {
        get => this.Flags.OpCode;
        set => this.Flags.OpCode = value;
    }

    public ResponseCode ResponseCode
    {
        get => this.Flags.ResponseCode;
        set => this.Flags.ResponseCode = value;
    }
    
    public void Serialize(Span<byte> bytes, ref ushort index, Dictionary<string, ushort> domainNameOffsetCache)
    {
        BinaryPrimitives.WriteUInt16BigEndian(bytes.Slice(index, 2), this.Id);
        index += 2;
        this.Flags.Serialize(bytes, ref index, domainNameOffsetCache);
        BinaryPrimitives.WriteUInt16BigEndian(bytes.Slice(index, 2), this.QuestionCount);
        BinaryPrimitives.WriteUInt16BigEndian(bytes.Slice(index + 2, 2), this.AnswerCount);
        BinaryPrimitives.WriteUInt16BigEndian(bytes.Slice(index + 4, 2), this.AuthorityCount);
        BinaryPrimitives.WriteUInt16BigEndian(bytes.Slice(index + 6, 2), this.AdditionalCount);
        index += 8;
    }

    public void Deserialize(ReadOnlySpan<byte> bytes, ref ushort offset)
    {
        this.Id = BinaryPrimitives.ReadUInt16BigEndian(bytes.Slice(offset, 2));
        offset += 2;
        this.Flags = new Flags();
        this.Flags.Deserialize(bytes, ref offset);
        this.QuestionCount = BinaryPrimitives.ReadUInt16BigEndian(bytes.Slice(offset, 2));
        this.AnswerCount = BinaryPrimitives.ReadUInt16BigEndian(bytes.Slice(offset + 2, 2));
        this.AuthorityCount = BinaryPrimitives.ReadUInt16BigEndian(bytes.Slice(offset + 4, 2));
        this.AdditionalCount = BinaryPrimitives.ReadUInt16BigEndian(bytes.Slice(offset + 6, 2));
        offset += 8;
    }

    public override string ToString()
        => $"{nameof(this.Id)}: {this.Id}, {nameof(this.Flags)}: {this.Flags}, {nameof(this.QuestionCount)}: {this.QuestionCount}, {nameof(this.AnswerCount)}: {this.AnswerCount}, {nameof(this.AuthorityCount)}: {this.AuthorityCount}, {nameof(this.AdditionalCount)}: {this.AdditionalCount}, {nameof(this.Query)}: {this.Query}, {nameof(this.OpCode)}: {this.OpCode}, {nameof(this.ResponseCode)}: {this.ResponseCode}";
}
