// SPDX-FileCopyrightText: 2023 - 2026 varelen and nerve contributors
// 
// SPDX-License-Identifier: BSD-3-Clause

using Microsoft.AspNetCore.Http.HttpResults;

using Nerve.Service.Domain;

namespace Nerve.Service.Endpoints;

public static class SystemEndpoints
{
    public static void AddSystemEndpoints(this WebApplication app)
    {
        app.MapPost("/api/system/restart", Restart);
    }

    public static Ok Restart(
        NerveDbContext dbContext,
        CancellationToken cancellationToken)
    {
        Program.Restart();

        return TypedResults.Ok();
    }
}
