using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace EJLive.Tests
{
    public class AgentControllerTests
    {
        [Fact]
        public void IAgentController_Exists()
        {
            var type = typeof(EJLive.Client.Service.IAgentController);
            Assert.True(type.IsInterface);
            Assert.Contains("StartAll", type.GetMethods().Select(m => m.Name));
            Assert.Contains("StopAll", type.GetMethods().Select(m => m.Name));
            Assert.Contains("GetStatus", type.GetMethods().Select(m => m.Name));
        }

        [Fact]
        public void AgentControllerState_AllValuesDefined()
        {
            var values = Enum.GetValues(typeof(EJLive.Client.Service.AgentControllerState));
            Assert.Equal(5, values.Length);
            Assert.Contains(EJLive.Client.Service.AgentControllerState.Failed, values.Cast<EJLive.Client.Service.AgentControllerState>());
        }
    }

    public class JournalContractsTests
    {
        [Fact]
        public void EjParserRegistry_ResolvesFallback()
        {
            var fallback = new EJLive.Core.Engine.PreservingFallbackEjTransactionParser();
            var registry = new EJLive.Core.Engine.EjParserRegistry(fallback);
            var parser = registry.Resolve("NonexistentVendor");
            Assert.Same(fallback, parser);
        }

        [Fact]
        public void GenericFallbackParser_PreservesAllLines()
        {
            var parser = new EJLive.Core.Engine.PreservingFallbackEjTransactionParser();
            var result = parser.Parse(new List<string> { "Line1", "Line2", "Line3" }, "ATM001");
            Assert.Single(result);
            Assert.Equal(3, result[0].RawLines.Count);
            Assert.Equal(EJLive.Core.Models.TransactionClassification.Suspicious, result[0].Classification);
            Assert.Equal(0, result[0].Confidence);
        }

        [Fact]
        public void EjTransaction_StatusEnum_ContainsAllRequired()
        {
            var type = typeof(EJLive.Core.Models.TransactionClassification);
            Assert.True(Enum.IsDefined(type, "Success"));
            Assert.True(Enum.IsDefined(type, "Failed"));
            Assert.True(Enum.IsDefined(type, "ApprovedNoDispense"));
            Assert.True(Enum.IsDefined(type, "PartialDispense"));
            Assert.True(Enum.IsDefined(type, "Reversal"));
            Assert.True(Enum.IsDefined(type, "CashJam"));
        }
    }

    public class NcrParserTests
    {
        [Fact]
        public void NcrParser_CanParse_DetectsTransactionStart()
        {
            var parser = new EJLive.Core.Engine.NcrEjTransactionParser();
            var result = parser.Parse(new List<string> { "SOME LOG LINE", "*TRANSACTION START* ATM001", "MORE DATA" }, "ATM001");
            Assert.Single(result);
        }

        [Fact]
        public void NcrParser_Success_RequiresNotesPresentedAndTaken()
        {
            var parser = new EJLive.Core.Engine.NcrEjTransactionParser();
            var result = parser.Parse(new List<string>
                {
                    "*TRANSACTION START*",
                    "CARD INSERTED",
                    "PIN ENTERED",
                    "NOTES PRESENTED",
                    "NOTES TAKEN",
                    "TRANSACTION END"
                }, "ATM001");
            Assert.Single(result);
            Assert.Equal(EJLive.Core.Models.TransactionClassification.Success, result[0].Classification);
            Assert.True(result[0].Confidence >= 0.9);
        }

        [Fact]
        public void NcrParser_ApprovedNoDispense_WhenArqcWithoutNotes()
        {
            var parser = new EJLive.Core.Engine.NcrEjTransactionParser();
            var result = parser.Parse(new List<string>
                {
                    "*TRANSACTION START*",
                    "CARD INSERTED",
                    "ARQC GENERATED",
                    "TRANSACTION END"
                }, "ATM001");
            Assert.Single(result);
            Assert.Equal(EJLive.Core.Models.TransactionClassification.ApprovedNoDispense, result[0].Classification);
        }
    }

    public class TransportContractsTests
    {
        [Fact]
        public void HandshakeRequest_HasRequiredFields()
        {
            var req = new EJLive.Core.Transport.HandshakeRequest
            {
                AtmId = "ATM001",
                MachineId = "MACH001",
                Vendor = "NCR"
            };
            Assert.Equal("ATM001", req.AtmId);
            Assert.NotEmpty(req.Nonce);
            Assert.True(req.TimestampUtc <= DateTimeOffset.UtcNow);
        }

        [Fact]
        public void HeartbeatPayload_Serializes()
        {
            var hb = new EJLive.Core.Transport.HeartbeatPayload
            {
                AtmId = "ATM001",
                SessionId = "SESS001",
                AgentState = "Running",
                OutboxCount = 5
            };
            var json = EJLive.Core.Transport.TransportSerializer.Serialize(hb);
            Assert.Contains("ATM001", json);
            Assert.Contains("SESS001", json);
            Assert.Contains("Running", json);
        }

        [Fact]
        public void TransferSession_IsComplete_FalseWhenPartial()
        {
            var session = new EJLive.Core.Transport.TransferSession
            {
                TotalChunks = 10,
                ReceivedChunks = new HashSet<int> { 0, 1, 2 }
            };
            Assert.False(session.IsComplete);
            Assert.Equal(3, session.NextExpectedOffset);
        }
    }

    public class VendorPathTests
    {
        [Fact]
        public void VendorPathRegistry_RegistersAllVendors()
        {
            var registry = new EJLive.Core.Vendors.VendorPathRegistry();
            registry.Register(new EJLive.Core.Vendors.NcrVendorPathProvider());
            registry.Register(new EJLive.Core.Vendors.GrgVendorPathProvider());
            registry.Register(new EJLive.Core.Vendors.WincorVendorPathProvider());
            registry.Register(new EJLive.Core.Vendors.DieboldVendorPathProvider());
            registry.Register(new EJLive.Core.Vendors.HyosungVendorPathProvider());
            registry.Register(new EJLive.Core.Vendors.CashwayVendorPathProvider());

            Assert.NotNull(registry.Resolve("NCR"));
            Assert.NotNull(registry.Resolve("GRG"));
            Assert.NotNull(registry.Resolve("Wincor"));
            Assert.NotNull(registry.Resolve("Diebold"));
            Assert.NotNull(registry.Resolve("Hyosung"));
            Assert.NotNull(registry.Resolve("Cashway"));
            Assert.Null(registry.Resolve("UnknownVendor"));
        }

        [Fact]
        public void NcrPathProvider_HasCorrectPaths()
        {
            var provider = new EJLive.Core.Vendors.NcrVendorPathProvider();
            Assert.Equal("NCR", provider.Vendor);
            Assert.NotEmpty(provider.JournalSourcePaths);
            Assert.NotEmpty(provider.JournalBackupPaths);
            Assert.True(provider.RequiresRestartForContentDeploy);
        }
    }

    public class SecurityTests
    {
        [Fact]
        public void SecretRedactor_MasksCardNumber()
        {
            var masked = EJLive.Core.Security.SecretRedactor.MaskCard("1234567890123456");
            Assert.Equal("123456****3456", masked);
        }

        [Fact]
        public void SecretRedactor_MasksAccount()
        {
            var masked = EJLive.Core.Security.SecretRedactor.MaskAccount("12345678");
            Assert.Equal("****5678", masked);
        }

        [Fact]
        public void UnifiedRemoteCommandPolicy_BlocksStaleTimestamp()
        {
            var policy = new EJLive.Core.Security.UnifiedRemoteCommandPolicy();
            var cmd = new EJLive.Core.Security.RemoteCommandEnvelope
            {
                TimestampUtc = DateTimeOffset.UtcNow.AddMinutes(-10)
            };
            var error = policy.Validate(cmd);
            Assert.NotNull(error);
            Assert.Contains("stale", error);
        }

        [Fact]
        public void UnifiedRemoteCommandPolicy_BlocksExpired()
        {
            var policy = new EJLive.Core.Security.UnifiedRemoteCommandPolicy();
            var cmd = new EJLive.Core.Security.RemoteCommandEnvelope
            {
                ExpiryUtc = DateTimeOffset.UtcNow.AddMinutes(-1)
            };
            var error = policy.Validate(cmd);
            Assert.NotNull(error);
            Assert.Contains("expired", error);
        }
    }

    public class XfsContractsTests
    {
        [Fact]
        public void NormalizedVendorEvent_HasAllFields()
        {
            var evt = new EJLive.Core.Xfs.XfsNormalizedEvent
            {
                Vendor = "NCR",
                Component = "CDM",
                EventCode = "ERR001",
                Message = "Cash jam detected"
            };
            Assert.NotEmpty(evt.EventId);
            Assert.Equal("NCR", evt.Vendor);
        }

        [Fact]
        public void CorrelationEngine_StrongMatch_ByStan()
        {
            var engine = new EJLive.Core.Engine.CorrelationEngine();
            var now = DateTime.UtcNow;
            var tx = new EJLive.Core.Models.EjTransaction(
                "tx-1", 0, 5, "ATM001", string.Empty, string.Empty, 100, "SAR",
                "123456", string.Empty, null, null, null, null, string.Empty, string.Empty,
                new List<string>(), EJLive.Core.Models.TransactionClassification.Success, 1, now);
            var evt = new EJLive.Core.Models.NormalizedVendorEvent(
                "evt-1", "ATM001", "NCR", "CashDispenser", "Info", "OK",
                "Dispense complete", now, "STAN:123456 Dispense complete", "trace.log",
                1, string.Empty, string.Empty, 1, string.Empty);
            var correlated = engine.Correlate(
                new List<EJLive.Core.Models.NormalizedVendorEvent> { evt },
                new List<EJLive.Core.Models.EjTransaction> { tx });
            Assert.Single(correlated);
            Assert.Equal("tx-1", correlated[0].ImpactedTransactionId);
            Assert.StartsWith("Strong match", correlated[0].CorrelationReason);
        }

        [Fact]
        public void NcrMergedTraceCorrelation_OrdersSourcesAndBuildsCounts()
        {
            var late = new EJLive.Core.Xfs.XfsNormalizedEvent
            {
                SourceLayer = EJLive.Core.Xfs.XfsSourceLayer.XfsStatus,
                Kind = EJLive.Core.Xfs.XfsEventKind.DeviceStatus,
                RawLine = "2026-07-12 10:00:02 CDM ready",
                Message = "CDM ready",
                Data = new Dictionary<string, string> { ["device"] = "CDM" }
            };
            var early = new EJLive.Core.Xfs.XfsNormalizedEvent
            {
                SourceLayer = EJLive.Core.Xfs.XfsSourceLayer.HostTransport,
                Kind = EJLive.Core.Xfs.XfsEventKind.HostReply,
                RawLine = "12/07/26 10:00:01 HOST APPROVED",
                Message = "Approved"
            };
            var sources = new (string SourceName, IReadOnlyList<EJLive.Core.Xfs.XfsNormalizedEvent> Events, int RawLines)[]
            {
                ("XFS", new[] { late }, 4),
                ("HOST", new[] { early }, 3)
            };

            var result = new EJLive.Core.Services.NcrMergedTraceCorrelationService().Correlate(sources);

            Assert.Equal(7, result.TotalRawLines);
            Assert.Equal(2, result.TotalEvents);
            Assert.Equal("HOST", result.Timeline[0].SourceName);
            Assert.Equal(1, result.BySourceLayer[nameof(EJLive.Core.Xfs.XfsSourceLayer.HostTransport)]);
            Assert.Equal("CDM", result.Timeline[1].Data["device"]);
        }
    }

    public class IngestionPipelineTests
    {
        [Fact]
        public void IngestionRecord_HasAllStages()
        {
            var stages = Enum.GetValues(typeof(EJLive.Core.Server.IngestionStage)).Cast<EJLive.Core.Server.IngestionStage>().ToList();
            Assert.Contains(EJLive.Core.Server.IngestionStage.Received, stages);
            Assert.Contains(EJLive.Core.Server.IngestionStage.Parsed, stages);
            Assert.Contains(EJLive.Core.Server.IngestionStage.SnapshotReady, stages);
            Assert.Contains(EJLive.Core.Server.IngestionStage.Failed, stages);
        }

        [Fact]
        public void ArchivePathStrategy_GeneratesMonthPath()
        {
            var ts = new DateTimeOffset(2026, 5, 26, 10, 0, 0, TimeSpan.Zero);
            var path = EJLive.Core.Server.ArchivePathStrategy.BuildPath(@"C:\Archive", "ATM001", "journal.log", ts);
            Assert.Contains("2026-05", path);
            Assert.Contains("ATM001", path);
            Assert.Contains("journal.log", path);
        }
    }
}
