using System;
  using System.Text;

  namespace EJLive.Core.Communication;

  // ── Security & Network Constants ─────────────────────────────────────────────

  /// <summary>
  /// Cryptographic defaults used by the EJLive protocol layer.
  /// Override via secure configuration — do not rely on these in production.
  /// </summary>
  public static class SecurityConfig
  {
      public const string DEFAULT_KEY = "EJLiveEnterprise2026AESKey123456";
      public const string DEFAULT_IV  = "EJLive2026AESIV!";
  }

  /// <summary>
  /// Transport-level tuning constants for the TCP socket layer.
  /// </summary>
  public static class NetworkConfig
  {
      public const int CONNECTION_TIMEOUT_MS  = 30_000;
      public const int SOCKET_BUFFER_SIZE     = 65_536;
      public const int PING_TIMEOUT_MS        = 5_000;
      public const int HEARTBEAT_INTERVAL_MS  = 30_000;
      public const int MAX_MESSAGE_SIZE       = 1_024 * 1_024;
  }

  /// <summary>
  /// Well-known NCR EJ file targets collected here for single-source access.
  /// </summary>
  public static class NCRFiles
  {
      public static readonly string[] TargetFiles = { "EJDATA.LOG", "EJRCPY.LOG", "EJDATA.LOb" };
  }

  /// <summary>
  /// Glob patterns used to discover vendor-specific EJ source files.
  /// </summary>
  public static class FilePatterns
  {
      public const string GRG_EJ_PATTERN    = "EJ*.*";
      public const string GRG_TRACE_PATTERN = "TRACE*.*";
      public const string WN_EJ_PATTERN     = "*.ej";
  }

  // ── Message Protocol ─────────────────────────────────────────────────────────

  /// <summary>
  /// Pipe-delimited message protocol for EJLive client-server communication.
  /// Provides message type constants, builder helpers, and a parser.
  /// </summary>
  public static class Protocol
  {
      // Handshake / session
      public const string HANDSHAKE        = "EJLIVE_HANDSHAKE";
      public const string HANDSHAKE_ACK    = "EJLIVE_ACK";
      public const string ACK              = "EJLIVE_ACK";      // Alias for backward compat
      public const string HANDSHAKE_REJECT = "EJLIVE_REJECT";

      // Heartbeat
      public const string HEARTBEAT        = "HEARTBEAT";
      public const string HEARTBEAT_ACK    = "HB_ACK";

      // Data transfer
      public const string DATA_JOURNAL     = "EJDATA";
      public const string EJDATA           = "EJDATA";          // Alias
      public const string DATA_FILE        = "EJFILE";
      public const string DATA_ACK         = "DATA_ACK";

      // Status
      public const string STATUS_REQUEST   = "STATUS_REQ";
      public const string STATUS_RESPONSE  = "STATUS_RES";

      // Remote commands
      public const string CMD_PING         = "CMD_PING";
      public const string CMD_RESULT       = "CMD_RESULT";
      public const string CMD_RESTART      = "CMD_RESTART";
      public const string CMD_SCREENSHOT   = "CMD_SCREENSHOT";
      public const string CMD_SYNC_TIME    = "CMD_SYNC_TIME";
      public const string CMD_TIMESYNC     = "CMD_SYNC_TIME";   // Alias
      public const string CMD_SHUTDOWN     = "CMD_SHUTDOWN";
      public const string CMD_CHANGE_PASSWORD = "CMD_CHANGE_PASSWORD";
      public const string CMD_SEND_IMAGE   = "CMD_SEND_IMAGE";
      public const string CMD_UPDATE_CONFIG= "CMD_REMOTE_CONFIG";
      public const string CMD_GET_SYSINFO  = "CMD_GET_STATS";

      // Framing
      public const string HEADER_END       = "\n";
      public const string DATA_END         = "\n<<END>>";

      // ── Builder helpers ───────────────────────────────────────────────────────

      /// <summary>Builds a pipe-delimited message: <c>messageType|part1|part2…</c></summary>
      public static string BuildMessage(string messageType, params string[] parts)
      {
          if (parts == null || parts.Length == 0)
              return messageType ?? string.Empty;

          var values = new string[parts.Length + 1];
          values[0] = messageType ?? string.Empty;
          Array.Copy(parts, 0, values, 1, parts.Length);
          return string.Join("|", values);
      }

      /// <summary>Overload accepting an explicit array — retains backward compatibility.</summary>
      public static string BuildMessage(string command, string[] parameters, bool unused)
      {
          var sb = new StringBuilder();
          sb.Append(command);
          if (parameters != null)
              foreach (var p in parameters) { sb.Append('|'); sb.Append(p); }
          return sb.ToString();
      }

      /// <summary>Parses a pipe-delimited message into its components.</summary>
      public static string[] ParseMessage(string message)
      {
          if (string.IsNullOrWhiteSpace(message))
              return Array.Empty<string>();
          return message.Trim().Split('|');
      }

      /// <summary>Builds a handshake message containing ATM identity.</summary>
      public static string BuildHandshakeMessage(string atmId, string atmType)
          => $"{HANDSHAKE}|{atmId}|{atmType}";

      /// <summary>Builds an EJ data message for legacy consumers.</summary>
      public static string BuildEJDataMessage(string fileName, string fileData)
          => $"{EJDATA}|{fileName}|{fileData}";

      /// <summary>Builds a remote command message with optional parameters.</summary>
      public static string BuildCommandMessage(string command, string[]? parameters = null)
          => BuildMessage(command, parameters ?? Array.Empty<string>());
  }
  