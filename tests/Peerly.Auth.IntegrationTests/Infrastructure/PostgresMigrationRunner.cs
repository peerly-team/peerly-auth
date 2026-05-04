using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;

namespace Peerly.Auth.IntegrationTests.Infrastructure;

internal static class PostgresMigrationRunner
{
    public static async Task ApplyAsync(NpgsqlDataSource dataSource)
    {
        var migrationsDirectory = Path.Combine(
            RepositoryRootProvider.GetRepositoryRoot(),
            "src",
            "Peerly.Auth.Persistence",
            "Migrations");

        await using var connection = await dataSource.OpenConnectionAsync();

        var migrationFiles = Directory
            .EnumerateFiles(migrationsDirectory, "*.sql")
            .OrderBy(file => file);

        await using var transaction = await connection.BeginTransactionAsync();
        try
        {
            foreach (var migrationFile in migrationFiles)
            {
                var lines = await File.ReadAllLinesAsync(migrationFile);
                var migrationSql = ExtractUpMigration(lines);
                if (string.IsNullOrWhiteSpace(migrationSql))
                {
                    continue;
                }

                await using var command = new NpgsqlCommand(migrationSql, connection);
                await command.ExecuteNonQueryAsync();
            }
            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static string ExtractUpMigration(string[] lines)
    {
        var upStartIndex = Array.FindIndex(lines, line => line.Trim() == "-- +goose Up");
        if (upStartIndex < 0)
        {
            throw new InvalidOperationException("Migration does not contain a goose Up section.");
        }

        var downStartIndex = Array.FindIndex(lines, upStartIndex + 1, line => line.Trim() == "-- +goose Down");
        var upLines = lines
            .Skip(upStartIndex + 1)
            .Take(downStartIndex < 0 ? lines.Length : downStartIndex - upStartIndex - 1)
            .Where(line => !line.TrimStart().StartsWith("-- +goose", StringComparison.Ordinal));

        return string.Join(Environment.NewLine, upLines);
    }
}
