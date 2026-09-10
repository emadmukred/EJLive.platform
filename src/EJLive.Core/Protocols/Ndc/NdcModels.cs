using System;
using System.Collections.Generic;

namespace EJLive.Core.Protocols.Ndc
{
    public enum NdcMessageDirection { TerminalToCentral, CentralToTerminal }
    public enum NdcMessageType { Startup, ReadyStatus, TransactionRequest, TransactionReply, SolicitedStatus, UnsolicitedStatus, DeviceStatus, FaultStatus, EJUpload, ConfigurationLoad, TerminalCommand, CommunicationLost, CommunicationRestored }

    public sealed class NdcMessage
    {
        public string MessageId { get; set; } = Guid.NewGuid().ToString("N");
        public string ATM_ID { get; set; } = string.Empty;
        public NdcMessageDirection Direction { get; set; }
        public NdcMessageType Type { get; set; }
        public string RawPayload { get; set; } = string.Empty;
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
        public string? SwitchResponseCode { get; set; }
    }

    public sealed class NdcSession
    {
        public string SessionId { get; set; } = string.Empty;
        public string ATM_ID { get; set; } = string.Empty;
        public DateTime StartedUtc { get; set; }
        public DateTime? LastActivityUtc { get; set; }
        public List<NdcMessage> Messages { get; set; } = new();
        public bool IsActive { get; set; }
    }

    public sealed class NdcSwitchState
    {
        public string ATM_ID { get; set; } = string.Empty;
        public bool IsConnected { get; set; }
        public DateTime? LastRequestUtc { get; set; }
        public DateTime? LastReplyUtc { get; set; }
        public string? LastHostResponse { get; set; }
        public int ConsecutiveErrors { get; set; }
        public string State => IsConnected ? (ConsecutiveErrors > 3 ? "Degraded" : "Online") : "Offline";
    }
}
