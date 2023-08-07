// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Dns;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc1035#section-4.1.1
/// </summary>
public sealed class Flags
{
    public bool QueryResponse
    {
        get => (this.flags & 0x8000) == 0x8000;
        set
        {
            if (value)
            {
                this.flags = (ushort)(this.flags | 0x8000);
            }
            else
            {
                this.flags = (ushort)(this.flags & (~0x8000));
            }
        }
    }

    public OpCode OpCode
    {
        get => (OpCode)(this.flags & 0x7800);
        set => this.flags = (ushort)((this.flags & ~0x7800) | ((byte)value));
    }

    public bool AuthoritativeAnswer
    {
        get => (this.flags & 0x0400) == 0x0400;
        set
        {
            if (value)
            {
                this.flags = (ushort)(this.flags | 0x0400);
            }
            else
            {
                this.flags = (ushort)(this.flags & (~0x0400));
            }
        }
    }

    public bool Truncation
    {
        get => (this.flags & 0x0200) == 0x0200;
        set
        {
            if (value)
            {
                this.flags = (ushort)(this.flags | 0x0200);
            }
            else
            {
                this.flags = (ushort)(this.flags & (~0x0200));
            }
        }
    }

    public bool RecursionDesired
    {
        get => (this.flags & 0x0100) == 0x0100;
        set
        {
            if (value)
            {
                this.flags = (ushort)(this.flags | 0x0100);
            }
            else
            {
                this.flags = (ushort)(this.flags & (~0x0100));
            }
        }
    }

    public bool RecursionAvailable
    {
        get => (this.flags & 0x0080) == 0x0080;
        set
        {
            if (value)
            {
                this.flags = (ushort)(this.flags | 0x0080);
            }
            else
            {
                this.flags = (ushort)(this.flags & (~0x0080));
            }
        }
    }

    public bool Zero 
    {
        get => (this.flags & 0x0040) == 0x0040;
        set
        {
            if (value)
            {
                this.flags = (ushort)(this.flags | 0x0040);
            }
            else
            {
                this.flags = (ushort)(this.flags & (~0x0040));
            }
        }
    }
    
    public ResponseCode ResponseCode
    {
        get => (ResponseCode)(this.flags & 0x000F);
        set => this.flags = (ushort)((this.flags & ~0x000F) | (byte)value);
    }

    private ushort flags;

    private bool Equals(Flags other)
        => this.flags == other.flags;

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
        
        return other.GetType() == this.GetType() && this.Equals((Flags)other);
    }

    public override int GetHashCode()
        => this.flags.GetHashCode();

    public override string ToString()
        => $"{nameof(this.QueryResponse)}: {this.QueryResponse}, {nameof(this.OpCode)}: {this.OpCode}, {nameof(this.AuthoritativeAnswer)}: {this.AuthoritativeAnswer}, {nameof(this.Truncation)}: {this.Truncation}, {nameof(this.RecursionDesired)}: {this.RecursionDesired}, {nameof(this.RecursionAvailable)}: {this.RecursionAvailable}, {nameof(this.Zero)}: {this.Zero}, {nameof(this.ResponseCode)}: {this.ResponseCode}";
}
