// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Dns;

public class Message : INetworkSerializable
{
    public Header Header { get; set; } = new();

    public List<Question> Questions { get; set; } = new List<Question>();
    
    public List<ResourceRecord> Answers { get; set; } = new List<ResourceRecord>();
    public List<ResourceRecord> Authorities { get; set; } = new List<ResourceRecord>();
    public List<ResourceRecord> Additionals { get; set; } = new List<ResourceRecord>();

    public void Serialize(Span<byte> bytes, ref ushort index)
    {
        this.Header.Serialize(bytes, ref index);
        
        foreach (Question question in this.Questions)
        {
           question.Serialize(bytes, ref index);
        }
        
        foreach (ResourceRecord answer in this.Answers)
        {
            answer.Serialize(bytes, ref index);
        }
        
        foreach (ResourceRecord authority in this.Authorities)
        {
            authority.Serialize(bytes, ref index);
        }

        foreach (ResourceRecord additional in this.Additionals)
        {
            additional.Serialize(bytes, ref index);
        }
    }

    public void Deserialize(ReadOnlySpan<byte> bytes, ref ushort offset)
    {
        this.Header.Deserialize(bytes, ref offset);
        
        for (ushort i = 0; i < this.Header.QuestionCount; i++)
        {
            var question = new Question();
            question.Deserialize(bytes, ref offset);
            this.Questions.Add(question);
        }
        
        for (ushort i = 0; i < this.Header.AnswerCount; i++)
        {
            var answerResourceRecord = new ResourceRecord();
            answerResourceRecord.Deserialize(bytes, ref offset);
            this.Answers.Add(answerResourceRecord);
        }
        
        for (ushort i = 0; i < this.Header.AuthorityCount; i++)
        {
            var authorityResourceRecord = new ResourceRecord();
            authorityResourceRecord.Deserialize(bytes, ref offset);
            this.Authorities.Add(authorityResourceRecord);
        }
        
        for (ushort i = 0; i < this.Header.AdditionalCount; i++)
        {
            var additionalResourceRecord = new ResourceRecord();
            additionalResourceRecord.Deserialize(bytes, ref offset);
            this.Additionals.Add(additionalResourceRecord);
        }
    }

    public override string ToString() 
        => $"{nameof(this.Header)}: {this.Header}, {nameof(this.Questions)}: {string.Join(", ", this.Questions)}, {nameof(this.Answers)}: {string.Join(", ", this.Answers)}, {nameof(this.Authorities)}: {string.Join(", ", this.Authorities)}, {nameof(this.Additionals)}: {string.Join(", ", this.Additionals)}";
}
