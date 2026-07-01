// SPDX-FileCopyrightText: 2023 - 2026 varelen and nerve contributors
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.ComponentModel.DataAnnotations;

namespace Nerve.Service;

public class NerveOptions
{
    public const string SectionName = "Nerve";

    public string Ip { get; set; } = "127.0.0.1";

    [Range(ushort.MinValue, ushort.MaxValue)]
    public int Port { get; set; } = 53;
    
    public PrivacyMode PrivacyMode { get; set; } = PrivacyMode.Everything;
}
