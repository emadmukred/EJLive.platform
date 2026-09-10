using System;
using System.Collections.Generic;

namespace EJLive.Core.Xfs
{
    public enum XfsVendor
    {
        Unknown,
        NCR,
        GRG,
        WN,
        Hyosung,
        CashWay,
        Diebold,
        Nautilus,
        DelaRue
    }

    public enum XfsSourceLayer
    {
        BusinessJournal,
        XfsStatus,
        DriverError,
        HardwareDiagnostic,
        HostTransport,
        MiddlewareRuntime,
        Configuration
    }

    public enum XfsEventKind
    {
        Unknown,
        TerminalModeTransition,
        NetworkState,
        TransactionLifecycle,
        TransactionRequest,
        HostReply,
        TransactionSerial,
        DeviceStatus,
        DeviceFault,
        CassetteSnapshot,
        CassetteChange,
        CashDispense,
        CashDeposit,
        Retract,
        CardEvent,
        CardReaderFlow,
        CardReaderFitness,
        CardReaderStatistics,
        PrinterEvent,
        Timeout,
        OcrCapture,
        CommandReject,
        Maintenance,
        HostMessageInbound,
        HostMessageOutbound,
        ProtocolKeepalive,
        XfsSessionLifecycle,
        XfsGetInfoCycle,
        XfsCapabilitiesQuery,
        XfsStatusPolling,
        VirtualControllerInbound,
        VirtualControllerOutbound,
        TerminalCommandLifecycle,
        MiddlewareValidationFlow,
        HeartbeatTelemetry,
        UpsState,
        ConfigurationProfile
    }

    // Note: XfsSeverity, XfsNormalizedEvent, and XfsCassetteSnapshot are defined in XfsModels.cs.
    // This file provides extended classification enums (XfsVendor, XfsSourceLayer, XfsEventKind)
    // that complement the base models without duplication.
}
