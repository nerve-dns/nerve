// SPDX-FileCopyrightText: 2023 - 2026 varelen and nerve contributors
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Service;

public interface IListService
{
    public Task RefreshAsync(int listId, CancellationToken cancellationToken);
    
    public Task LoadListsAsync(CancellationToken cancellationToken);
}
