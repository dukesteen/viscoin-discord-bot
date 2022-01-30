using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Viscoin.Bot.Infrastructure.Data;

public static class DapperDbContextExtensions
{
    public static async Task<IEnumerable<T>> QueryAsync<T>(
        this DbContext context,
        string sql,
        object? parameters = null,
        CancellationToken ct = default,
        int? timeout = null,
        CommandType? type = null
    )
    {
        using var command = new DapperEfCoreCommand(
            context,
            sql,
            parameters!,
            timeout,
            type,
            ct
        );

        var connection = context.Database.GetDbConnection();
        return await connection.QueryAsync<T>(command.Definition);
    }

    public static async Task<int> ExecuteAsync(
        this DbContext context,
        CancellationToken ct,
        string text,
        object? parameters = null,
        int? timeout = null,
        CommandType? type = null
    )
    {
        using var command = new DapperEfCoreCommand(
            context,
            text,
            parameters,
            timeout,
            type,
            ct
        );

        var connection = context.Database.GetDbConnection();
        return await connection.ExecuteAsync(command.Definition);
    }
}

public readonly struct DapperEfCoreCommand : IDisposable
{
    private readonly ILogger<DapperEfCoreCommand> _logger;

    public DapperEfCoreCommand(
        DbContext context,
        string text,
        object? parameters,
        int? timeout,
        CommandType? type,
        CancellationToken ct
    )
    {
        _logger = context.GetService<ILogger<DapperEfCoreCommand>>();

        var transaction = context.Database.CurrentTransaction?.GetDbTransaction();
        var commandType = type ?? CommandType.Text;
        var commandTimeout = timeout ?? context.Database.GetCommandTimeout() ?? 30;

        Definition = new CommandDefinition(
            text,
            parameters,
            transaction,
            commandTimeout,
            commandType,
            cancellationToken: ct
        );

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug(
                @"Executing DbCommand [CommandType='{CommandType}', CommandTimeout='{CommandTimeout}']
{CommandText}", Definition.CommandType, Definition.CommandTimeout, Definition.CommandText);
        }
    }

    public CommandDefinition Definition { get; }

    public void Dispose()
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation(
                @"Executed DbCommand [CommandType='{CommandType}', CommandTimeout='{CommandTimeout}']
{CommandText}", Definition.CommandType, Definition.CommandTimeout, Definition.CommandText);
        }
    }
}