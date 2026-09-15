using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using EJLive.Core.Data;
using EJLive.Core.Data.Repositories;
using EJLive.Core.Engine;
using EJLive.Core.Services;
using Microsoft.Data.Sqlite;
using Xunit;

namespace EJLive.Tests;

/// <summary>
/// Wave 4 (SS-20) — dataroot, constants unification and the Excel writer. These cases do
/// not touch the shared database; they pin the value-unification work (Core facade == Shared
/// canonical, vendor-code aliases, protocol verbs) and the dependency-free xlsx output.
/// </summary>
public sealed class Wave4ConstantsAndWorkbookTests
{
    [Fact]
    public void DataRootPaths_Resolve_PrefersAbsoluteOverrideAndFallsBackSafely()
    {
        var probeRoot = Path.Combine(Path.GetTempPath(), "ejlive-dataroot-probe-" + Guid.NewGuid().ToString("N"));
        try
        {
            Assert.Equal(probeRoot, EJLive.Shared.DataRootPaths.Resolve(probeRoot));
            Assert.Equal(EJLive.Shared.DataRootPaths.Resolve(null), EJLive.Shared.DataRootPaths.Resolve("   "));        // blank → default
            Assert.Equal(EJLive.Shared.DataRootPaths.Resolve(null), EJLive.Shared.DataRootPaths.Resolve("relative/x")); // relative → default
            Assert.False(EJLive.Shared.DataRootPaths.IsUsablePath("C:\\bad*wild"));
            Assert.True(EJLive.Shared.DataRootPaths.IsUsablePath(probeRoot));
        }
        finally
        {
            if (Directory.Exists(probeRoot)) Directory.Delete(probeRoot, recursive: true);
        }
    }

    [Fact]
    public void DataRootLayout_RejectsRelativeRoots()
    {
        Assert.Throws<ArgumentException>(() => new DataRootLayout("relative\\root"));
        Assert.Throws<ArgumentException>(() => new DataRootLayout("  "));
    }

    [Fact]
    public void AppConstants_CoreFacade_MatchesSharedCanonicalValues()
    {
        // The facade exists to keep L1+ files resolving the single source of truth; any
        // drift between the two classes fails this pin.
        Assert.Equal(EJLive.Shared.AppConstants.AppVersion, EJLive.Core.AppConstants.AppVersion);
        Assert.Equal(EJLive.Shared.AppConstants.DefaultPort, EJLive.Core.AppConstants.DefaultPort);
        Assert.Equal(EJLive.Shared.AppConstants.CMD_RESTART, EJLive.Core.AppConstants.CMD_RESTART);
        Assert.Equal(EJLive.Shared.AppConstants.ATM_TYPE_GRG, EJLive.Core.AppConstants.ATM_TYPE_GRG);
        Assert.Equal(EJLive.Shared.AppConstants.HeartbeatIntervalSec, EJLive.Core.AppConstants.HeartbeatIntervalSec);
        Assert.Equal(EJLive.Shared.AppConstants.DefaultDatabasePath, EJLive.Core.AppConstants.DefaultDatabasePath);
    }

    [Fact]
    public void AppConstants_VendorCodes_AreDistinctAndNormalizeBothAliasStyles()
    {
        var codes = new[]
        {
            EJLive.Core.AppConstants.ATM_TYPE_NCR,
            EJLive.Core.AppConstants.ATM_TYPE_GRG,
            EJLive.Core.AppConstants.ATM_TYPE_WN,
            EJLive.Core.AppConstants.ATM_TYPE_DN,
            EJLive.Core.AppConstants.ATM_TYPE_HY,
            EJLive.Core.AppConstants.ATM_TYPE_CW
        };
        Assert.Equal(codes.Length, codes.Distinct(StringComparer.OrdinalIgnoreCase).Count());

        Assert.Equal(EJLive.Core.AppConstants.ATM_TYPE_WN, EJLive.Core.AppConstants.NormalizeATMType("Wincor"));
        Assert.Equal(EJLive.Core.AppConstants.ATM_TYPE_WN, EJLive.Core.AppConstants.NormalizeATMType("WN"));
        Assert.Equal(EJLive.Core.AppConstants.ATM_TYPE_DN, EJLive.Core.AppConstants.NormalizeATMType("Diebold"));
        Assert.Equal(EJLive.Core.AppConstants.ATM_TYPE_DN, EJLive.Core.AppConstants.NormalizeATMType("DN"));
        Assert.Equal(EJLive.Core.AppConstants.ATM_TYPE_CW, EJLive.Core.AppConstants.NormalizeATMType("CASHWAY"));
        Assert.Equal(EJLive.Core.AppConstants.ATM_TYPE_CW, EJLive.Core.AppConstants.NormalizeATMType("cw"));
        Assert.Equal(EJLive.Core.AppConstants.ATM_TYPE_NCR, EJLive.Core.AppConstants.NormalizeATMType(null)); // legacy default kept
    }

    [Fact]
    public void Protocol_FacadeVerbs_MatchWireConstants()
    {
        Assert.Equal(EJLive.Core.AppConstants.MSG_HANDSHAKE, EJLive.Core.Protocol.HANDSHAKE);
        Assert.Equal(EJLive.Core.AppConstants.MSG_JOURNAL_ACK, EJLive.Core.Protocol.DATA_ACK);
        Assert.Equal(EJLive.Core.AppConstants.CMD_RESTART, EJLive.Core.Protocol.CMD_RESTART);
    }

    [Fact]
    public void ExcelWorkbookWriter_ProducesOpenablePackageWithEscapedAndNumericCells()
    {
        var path = Path.Combine(Path.GetTempPath(), $"ejlive-wave4-{Guid.NewGuid():N}.xlsx");
        try
        {
            var sheet = new ExcelSheet(
                "Data [1]",
                new[] { "name", "amount" },
                new List<IReadOnlyList<string?>>
                {
                    new string?[] { "cash & carry <x>", "123.45" },
                    new string?[] { "plain", "not-a-number" }
                });
            ExcelWorkbookWriter.Write(path, sheet);

            Assert.True(File.Exists(path));
            using var zip = ZipFile.OpenRead(path);
            Assert.NotNull(zip.GetEntry("[Content_Types].xml"));
            Assert.NotNull(zip.GetEntry("xl/workbook.xml"));
            Assert.NotNull(zip.GetEntry("xl/worksheets/sheet1.xml"));

            using var reader = new StreamReader(zip.GetEntry("xl/worksheets/sheet1.xml")!.Open());
            var xml = reader.ReadToEnd();
            Assert.Contains("cash &amp; carry &lt;x&gt;", xml);   // XML escaping, inline string
            Assert.Contains("<v>123.45</v>", xml);                // plain numerals stay numeric
            Assert.Contains("not-a-number", xml);
            Assert.Contains("Data  1", xml);                      // forbidden sheet chars sanitised
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void ExcelWorkbookWriter_ColumnNamesFollowSpreadsheetNotation()
    {
        Assert.Equal("A", ExcelWorkbookWriter.ColumnName(0));
        Assert.Equal("Z", ExcelWorkbookWriter.ColumnName(25));
        Assert.Equal("AA", ExcelWorkbookWriter.ColumnName(26));
    }
}

/// <summary>
/// The bootstrap machine runs on the shared database, so it is serialized in the
/// Wave4 data-access collection with every other fixture that mutates the central SQLite
/// file (the migration book is not safe to apply concurrently from two runners).
/// </summary>
[Collection("Wave4DataAccess")]
public sealed class Wave4BootstrapTests
{
    [Fact]
    public async Task Bootstrap_RunsEveryStepAgainstTheCentralDataroot()
    {
        // Uses the default (central) dataroot + database so it cannot re-bind the shared
        // DatabaseManager singleton to a disposable path under another fixture's feet;
        // the whole point of the central root is that every process converges on it.
        var configPath = Path.Combine(Path.GetTempPath(), $"ejlive-wave4-cfg-{Guid.NewGuid():N}.json");
        try
        {
            var bootstrap = new PlatformBootstrap(options: new BootstrapOptions { ConfigPath = configPath });
            var result = await bootstrap.RunAsync();

            Assert.True(result.Success, result.FailureDetail ?? "(none)");
            Assert.Contains("database: OK", string.Join("\n", result.ReportLines()));
            Assert.Contains("schema: OK", string.Join("\n", result.ReportLines()));
            Assert.True(bootstrap.MigrationsApplied >= 0);
            Assert.NotNull(bootstrap.Configuration);

            var second = await new PlatformBootstrap(options: new BootstrapOptions { ConfigPath = configPath }).RunAsync();
            Assert.True(second.Success, second.FailureDetail ?? "(none)");
            Assert.Equal(0, second.MigrationsApplied, "second run applies no new migrations");
        }
        finally
        {
            if (File.Exists(configPath)) File.Delete(configPath);
        }
    }
}

/// <summary>
/// Repository cases run against the process database (the shared
/// <see cref="DatabaseManager"/> singleton) so they are collected into one serialized
/// xUnit collection; every fixture scopes its rows by a per-run marker and cleans up.
/// </summary>
[Collection("Wave4DataAccess")]
public sealed class Wave4RepositoryTests : IDisposable
{
    private static readonly string Marker = "WAVE4-" + Guid.NewGuid().ToString("N");
    private readonly DatabaseManager _db = DatabaseManager.Instance;

    public Wave4RepositoryTests()
    {
        // Guarantee the canonical tables (journal_archive et al. plus the Phase-2 set)
        // exist regardless of which fixture ran first in the assembly.
        _db.Initialize();
        DatabaseMigrationsRunner.FromDatabaseFile(EJLive.Shared.AppConstants.DefaultDatabasePath).RunAll();
    }

    public void Dispose()
    {
        _db.ExecuteNonQuery("DELETE FROM journal_archive WHERE atm_id=$m", new SqliteParameter("$m", Marker));
        _db.ExecuteNonQuery("DELETE FROM atm_registry WHERE atm_id=$m", new SqliteParameter("$m", Marker));
        _db.ExecuteNonQuery("DELETE FROM command_audit WHERE operator_id=$m", new SqliteParameter("$m", Marker));
        _db.ExecuteNonQuery("DELETE FROM transfer_sessions WHERE transfer_id LIKE $m", new SqliteParameter("$m", Marker + "%"));
        CleanupAudit();
    }

    [Fact]
    public void JournalArchive_UpsertIsIdempotentAndSumsWindow()
    {
        var repo = new JournalArchiveRepository(_db);
        var record = new JournalArchiveRecord(
            EntryId: Guid.NewGuid().ToString("N"),
            AtmId: Marker,
            FileName: "EJDATA-0001.LOG",
            OriginalSize: 4096,
            CompressedSize: 3000,
            EncryptedSize: 3000,
            IsEncrypted: true,
            IsCompressed: true,
            Checksum: "abc",
            Sha256Hash: "sha-1",
            TransactionCount: 7,
            ArchivePath: @"C:\none",
            ReceivedAtUtc: DateTime.UtcNow);

        Assert.True(repo.TryInsert(record));
        Assert.False(repo.TryInsert(record with { EntryId = Guid.NewGuid().ToString("N") }), "resend on the same key must not create a second row");

        // Same terminal, different hash: distinct capture → new row.
        Assert.True(repo.TryInsert(record with { EntryId = Guid.NewGuid().ToString("N"), Sha256Hash = "sha-2", TransactionCount = 3 }));

        Assert.Equal(2L, repo.CountFiles(Marker, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1)));
        Assert.Equal(10L, repo.SumTransactions(Marker, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1)));
    }

    [Fact]
    public void CommandAudit_AppendOutcomeAndQuery()
    {
        var repo = new CommandAuditRepository(_db);
        var commandId = Marker + "-CMD";
        repo.Append(new CommandAuditRecord(
            AuditId: Guid.NewGuid().ToString("N"),
            CommandId: commandId,
            OperatorId: Marker,
            Action: "RESTART",
            DetailsJson: "{\"delay\":5}",
            TimestampUtc: DateTime.UtcNow,
            AtmId: Marker,
            ArgsHash: "deadbeef",
            Outcome: "Pending"));

        Assert.Equal(1, repo.RecordOutcome(commandId, "Completed", 250));

        var rows = repo.QueryRecent(Marker, 10);
        var row = Assert.Single(rows);
        Assert.Equal("Completed", row.Outcome);
        Assert.Equal(250, row.LatencyMs);
    }

    [Fact]
    public void TransferSession_BitmapSurvivesRoundTrip()
    {
        var repo = new TransferSessionRepository(_db);
        var bits = new BitArray(8);
        bits.Set(0, true); bits.Set(2, true); bits.Set(7, true);

        var transferId = Marker + "-XFER";
        repo.Save(new TransferSessionRecord(
            TransferId: transferId,
            AtmId: Marker,
            FileName: "big-file.ej",
            FileLength: 64 * 1024 * 1024,
            ChunkSize: 64 * 1024,
            TotalChunks: 8,
            ReceivedChunksCsv: TransferSessionRepository.EncodeBitmap(bits),
            FileSha256: null,
            NextExpectedOffset: 64 * 1024,
            CreatedUtc: DateTime.UtcNow,
            CompletedUtc: null));

        var loaded = repo.Load(transferId);
        Assert.NotNull(loaded);
        var decoded = TransferSessionRepository.DecodeBitmap(loaded!.ReceivedChunksCsv, loaded.TotalChunks);
        Assert.Equal(new[] { 0, 2, 7 }, Enumerable.Range(0, decoded.Length).Where(i => decoded.Get(i)).ToArray());

        repo.MarkCompleted(transferId, DateTime.UtcNow);
        Assert.NotNull(repo.Load(transferId)!.CompletedUtc);
    }

    [Fact]
    public void AtmRegistry_UpsertNeverMovesHeartbeatBackwards()
    {
        var repo = new ATMRegistryRepository(_db);
        var now = DateTime.UtcNow;
        repo.Upsert(new AtmRegistrationRecord(Marker, "Wave4 Terminal", "NCR", "10.0.0.9", now, now, now));
        repo.Upsert(new AtmRegistrationRecord(Marker, "Wave4 Terminal", "NCR", "10.0.0.9", now.AddMinutes(1), now.AddMinutes(-30), now.AddMinutes(-30)));

        var row = repo.Snapshot().Single(r => r.AtmId == Marker);
        Assert.True(row.LastHeartbeatUtc >= now, "stale heartbeat must not overwrite a fresher one");
    }

    [Fact]
    public void AuditChain_FreshRowsVerify_AndTamperIsDetected()
    {
        var action = Marker + "-CHAIN";
        _db.InsertAuditLog(action, Marker, Marker, "row-one");
        _db.InsertAuditLog(action, Marker, Marker, "row-two");

        var first = _db.VerifyAuditChain();
        Assert.True(first.Valid, first.Detail);
        Assert.True(first.VerifiedRows >= 2);

        // Tamper with the stored row (as an attacker with shell access to the file would);
        // the verifier must then report a broken link.
        _db.ExecuteNonQuery(
            "UPDATE audit_log SET details=$evil WHERE action=$act AND details=$det",
            new SqliteParameter("$evil", "row-TWO-tampered"),
            new SqliteParameter("$act", action),
            new SqliteParameter("$det", "row-one"));

        var after = _db.VerifyAuditChain();
        Assert.False(after.Valid);
        Assert.NotNull(after.FirstBrokenRowId);

        // Restore — the chain verifies again. The marker rows are removed in Dispose; they
        // are the newest chained rows, so no later row's prev-hash can dangle.
        _db.ExecuteNonQuery(
            "UPDATE audit_log SET details=$good WHERE action=$act AND details=$evil",
            new SqliteParameter("$good", "row-one"),
            new SqliteParameter("$act", action),
            new SqliteParameter("$evil", "row-TWO-tampered"));

        var restored = _db.VerifyAuditChain();
        Assert.True(restored.Valid, restored.Detail);
    }

    private void CleanupAudit()
        => _db.ExecuteNonQuery("DELETE FROM audit_log WHERE performed_by=$m OR atm_id=$m", new SqliteParameter("$m", Marker));
}

/// <summary>
/// Single canonical schema book after Wave 4 (SS-11): <c>schema_migrations</c> only, with
/// checksums, backfill from the retired <c>__migrations</c> table and the newer-on-disk
/// refusal guard. These cases run against their own SQLite files, never the shared one.
/// </summary>
public sealed class Wave4SchemaBookTests
{
    private string _root = string.Empty;

    private string DbPath => Path.Combine(_root, "schema.db");

    private void NewRoot() => _root = Path.Combine(Path.GetTempPath(), "ejlive-schema-" + Guid.NewGuid().ToString("N"));

    [Fact]
    public void RunAll_AppliesTheFullBookAndRecordsChecksums()
    {
        NewRoot();
        try
        {
            Directory.CreateDirectory(_root);
            var runner = DatabaseMigrationsRunner.FromDatabaseFile(DbPath);
            var applied = runner.RunAll();
            Assert.Equal(13, applied); // 6 legacy + 7 Phase-2
            Assert.Equal(13, runner.GetCurrentVersion());
            Assert.Equal(0, runner.RunAll()); // second run applies nothing

            using var conn = new SqliteConnection($"Data Source={DbPath}");
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(1) FROM schema_migrations WHERE length(checksum) = 64;";
            Assert.Equal(13L, Convert.ToInt64(cmd.ExecuteScalar()));

            cmd.CommandText = "SELECT COUNT(1) FROM sqlite_master WHERE type='table' AND name='parser_transactions';";
            Assert.Equal(1L, Convert.ToInt64(cmd.ExecuteScalar()));
        }
        finally
        {
            if (Directory.Exists(_root)) Directory.Delete(_root, recursive: true);
        }
    }

    [Fact]
    public void LegacyMigrationsBook_IsBackfilledThenDropped()
    {
        NewRoot();
        try
        {
            Directory.CreateDirectory(_root);
            using (var seed = new SqliteConnection($"Data Source={DbPath}"))
            {
                seed.Open();
                using var cmd = seed.CreateCommand();
                cmd.CommandText = @"
CREATE TABLE __migrations (version INTEGER PRIMARY KEY, name TEXT NOT NULL, applied_at TEXT NOT NULL);
INSERT INTO __migrations (version, name, applied_at) VALUES (1, 'InitialSchema', '2024-01-01 00:00:00');
INSERT INTO __migrations (version, name, applied_at) VALUES (2, 'AddCommandQueue', '2024-01-01 00:00:01');";
                cmd.ExecuteNonQuery();
            }

            var runner = DatabaseMigrationsRunner.FromDatabaseFile(DbPath);
            var applied = runner.RunAll();
            Assert.Equal(11, applied); // versions 1 and 2 were already in the (folded) book
            Assert.Equal(13, runner.GetCurrentVersion());

            using var conn = new SqliteConnection($"Data Source={DbPath}");
            conn.Open();
            using var check = conn.CreateCommand();
            check.CommandText = "SELECT COUNT(1) FROM sqlite_master WHERE type='table' AND name='__migrations';";
            Assert.Equal(0L, Convert.ToInt64(check.ExecuteScalar()));
            check.CommandText = "SELECT name FROM schema_migrations WHERE version = 1;";
            Assert.Equal("InitialSchema", Convert.ToString(check.ExecuteScalar()));
        }
        finally
        {
            if (Directory.Exists(_root)) Directory.Delete(_root, recursive: true);
        }
    }

    [Fact]
    public void NewerOnDiskSchema_RefusesToRun()
    {
        NewRoot();
        try
        {
            Directory.CreateDirectory(_root);
            var runner = DatabaseMigrationsRunner.FromDatabaseFile(DbPath);
            Assert.True(runner.RunAll() > 0);

            using (var conn = new SqliteConnection($"Data Source={DbPath}"))
            {
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO schema_migrations (version, name) VALUES (999, 'FutureSchema');";
                cmd.ExecuteNonQuery();
            }

            Assert.False(runner.TryEnsureCurrent(out int current, out int head));
            Assert.Equal(999, current);
            Assert.Equal(DatabaseMigrationsRunner.LatestVersion, head);
            Assert.Throws<InvalidOperationException>(() => runner.RunAll());
        }
        finally
        {
            if (Directory.Exists(_root)) Directory.Delete(_root, recursive: true);
        }
    }
}
