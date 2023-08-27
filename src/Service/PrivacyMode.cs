// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Service;

public enum PrivacyMode
{
    Everything,
    HideDomains,
    HideClients,
    HideDomainsAndClients,
    Anonymous
}
