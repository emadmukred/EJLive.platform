using Xunit;

namespace EJLive.Tests;

/// <summary>
/// Database migration contract tests. Client companion and headless-agent
/// contracts live in their dedicated Track03 and Track08 suites so the same
/// runtime behavior is not asserted by multiple test copies.
/// </summary>
public sealed class DatabaseMigrationTests
{
    [Fact]
    public void MigrationRunner_AcceptsNewMigrations()
    {
        var runner = new EJLive.Core.Data.DatabaseMigrationRunner();
        runner.Register(new EJLive.Core.Data.DbMigration { MigrationId = 1, Description = "Test", UpSql = "SELECT 1;" });
        var pending = runner.GetPending();
        Assert.Single(pending);
        Assert.False(runner.IsUpToDate);
    }

    [Fact]
    public void MigrationRunner_CommitAppliesMigrations()
    {
        var runner = new EJLive.Core.Data.DatabaseMigrationRunner();
        runner.Register(new EJLive.Core.Data.DbMigration { MigrationId = 1, Description = "T1", UpSql = "SELECT 1;" });
        runner.Register(new EJLive.Core.Data.DbMigration { MigrationId = 2, Description = "T2", UpSql = "SELECT 2;" });
        runner.CommitApplied(new[] { 1, 2 });
        Assert.Empty(runner.GetPending());
        Assert.True(runner.IsUpToDate);
    }

    [Fact]
    public void MigrationRunner_MarkAppliedPreventsReRegister()
    {
        var runner = new EJLive.Core.Data.DatabaseMigrationRunner();
        runner.MarkApplied(1);
        runner.Register(new EJLive.Core.Data.DbMigration { MigrationId = 1, Description = "AlreadyApplied", UpSql = "SELECT 1;" });
        Assert.Empty(runner.GetPending());
        Assert.True(runner.IsUpToDate);
    }

    [Fact]
    public void MigrationRunner_PartialCommit_LeavesUnapplied()
    {
        var runner = new EJLive.Core.Data.DatabaseMigrationRunner();
        runner.Register(new EJLive.Core.Data.DbMigration { MigrationId = 1, Description = "T1", UpSql = "SELECT 1;" });
        runner.Register(new EJLive.Core.Data.DbMigration { MigrationId = 2, Description = "T2", UpSql = "SELECT 2;" });
        runner.Register(new EJLive.Core.Data.DbMigration { MigrationId = 3, Description = "T3", UpSql = "SELECT 3;" });
        runner.CommitApplied(new[] { 1, 2 });
        Assert.Single(runner.GetPending());
        Assert.False(runner.IsUpToDate);
    }

    [Fact]
    public void GenerateRequiredMigrations_ReturnsSeven()
    {
        var migrations = EJLive.Core.Data.DatabaseMigrationRunner.GenerateRequiredMigrations();
        Assert.Equal(7, migrations.Count);
    }

    [Fact]
    public void RequiredMigrations_HaveExecutableSqlAndDescriptions()
    {
        var migrations = EJLive.Core.Data.DatabaseMigrationRunner.GenerateRequiredMigrations();
        foreach (var migration in migrations)
        {
            Assert.False(string.IsNullOrWhiteSpace(migration.UpSql), $"Migration #{migration.MigrationId} has empty UpSql");
            Assert.False(string.IsNullOrWhiteSpace(migration.Description), $"Migration #{migration.MigrationId} has empty Description");
        }
    }
}
