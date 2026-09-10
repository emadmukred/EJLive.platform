using System.Text;
using System.Text.Json;
using EJLive.Core.Models;
using EJLive.Core.Services;
using EJLive.Shared;

namespace EJLive.Core.Engine;

public sealed class EJMessage
{
    public CommunicationProtocol.MsgType Type { get; set; }
    public string Text { get; set; } = string.Empty;
    public byte[] Payload { get; set; } = Array.Empty<byte>();
    public DateTime ReceivedAtUtc { get; set; } = DateTime.UtcNow;
}

public static class CommunicationProtocol
{
    public enum MsgType
    {
        Unknown,
        RsaPublicKey,
        AesSessionKey,
        Handshake,
        HandshakeAck,
        Heartbeat,
        HeartbeatAck,
        StartFile,
        Chunk,
        ChunkAck,
        Complete,
        JournalAck,
        Command,
        CommandResult,
        RemoteSessionStart,
        RemoteSessionFrame,
        RemoteSessionStop,
        ImageSync,
        ImageAck,
        Broadcast,
        Disconnect,
        Error
    }

    private static readonly Encoding WireEncoding = Encoding.UTF8;

    public static byte[] BuildRsaPublicKey(string publicKeyXml) => BuildFrame(MsgType.RsaPublicKey, publicKeyXml);
    public static byte[] BuildAesSessionKey(byte[] encryptedKey) => BuildFrame(MsgType.AesSessionKey, Convert.ToBase64String(encryptedKey));
    public static byte[] BuildHandshake(string atmId, string atmType, string version) => BuildFrame(MsgType.Handshake, $"{atmId}|{atmType}|{version}");
    public static byte[] BuildHandshakeAck(string sessionId) => BuildFrame(MsgType.HandshakeAck, $"OK|{sessionId}");
    public static byte[] BuildHeartbeat(string atmId, string payload = "") => BuildFrame(MsgType.Heartbeat, $"{atmId}|{payload}");
    public static byte[] BuildHeartbeatAck(string atmId = "") => BuildFrame(MsgType.HeartbeatAck, atmId);

    public static bool TryParseHeartbeatAck(string? text, out HeartbeatAck ack)
    {
        ack = new HeartbeatAck(DateTime.UtcNow, 0, false, text);
        var value = (text ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var separator = value.IndexOf('|');
        if (separator >= 0 && separator + 1 < value.Length)
            value = value[(separator + 1)..].Trim();
        if (!value.StartsWith('{'))
            return false;

        try
        {
            var parsed = JsonSerializer.Deserialize<HeartbeatAck>(value, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (parsed is null)
                return false;
            ack = parsed;
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
    public static byte[] BuildStartFile(string atmId, string fileName, long length, long offset, string checksum) => BuildFrame(MsgType.StartFile, $"{atmId}|{fileName}|{length}|{offset}|{checksum}");
    public static byte[] BuildChunk(int sequence, byte[] chunk, byte[]? sessionKey = null) => BuildFrame(MsgType.Chunk, sequence.ToString(), chunk, sessionKey);
    public static byte[] BuildChunkAck(int sequence) => BuildFrame(MsgType.ChunkAck, sequence.ToString(), BitConverter.GetBytes(sequence));
    public static byte[] BuildComplete(string fileName, string checksum, string sha256) => BuildFrame(MsgType.Complete, $"{fileName}|{checksum}|{sha256}");
    public static byte[] BuildJournalAck(string fileName, bool ok, string message = "") => BuildFrame(MsgType.JournalAck, $"{fileName}|{(ok ? "OK" : "FAIL")}|{message}");
    public static byte[] BuildCommand(RemoteCommandEnvelope command) => BuildFrame(MsgType.Command, command.ToWireText());
    public static byte[] BuildCommandResult(string commandId, bool ok, string message) => BuildFrame(MsgType.CommandResult, $"{commandId}|{(ok ? "OK" : "FAIL")}|{Convert.ToBase64String(Encoding.UTF8.GetBytes(message ?? string.Empty))}");
    public static byte[] BuildRemoteSessionStart(string atmId) => BuildFrame(MsgType.RemoteSessionStart, atmId);
    public static byte[] BuildRemoteSessionFrame(byte[] jpegFrame) => BuildFrame(MsgType.RemoteSessionFrame, "image/jpeg", jpegFrame);
    public static byte[] BuildRemoteSessionStop(string atmId) => BuildFrame(MsgType.RemoteSessionStop, atmId);
    public static byte[] BuildError(string message) => BuildFrame(MsgType.Error, message);
    public static byte[] BuildDisconnect(string reason = "") => BuildFrame(MsgType.Disconnect, reason);

    public static byte[] BuildFrame(MsgType type, string text, byte[]? payload = null, byte[]? sessionKey = null)
    {
        var body = payload is { Length: > 0 } ? payload : WireEncoding.GetBytes(text ?? string.Empty);
        if (sessionKey is { Length: > 0 })
            body = SecurityHelper.EncryptAES(body, sessionKey);

        var header = WireEncoding.GetBytes($"{type}:{body.Length}\n");
        var frame = new byte[header.Length + body.Length];
        Buffer.BlockCopy(header, 0, frame, 0, header.Length);
        Buffer.BlockCopy(body, 0, frame, header.Length, body.Length);
        return frame;
    }

    public static EJMessage ReadMessage(Stream stream, byte[]? sessionKey = null)
    {
        var header = ReadHeader(stream);
        var separator = header.IndexOf(':');
        if (separator < 0)
            throw new InvalidDataException("Invalid EJLive frame header.");

        var typeName = header[..separator];
        var lengthText = header[(separator + 1)..];
        if (!Enum.TryParse<MsgType>(typeName, out var type))
            type = MsgType.Unknown;
        if (!int.TryParse(lengthText, out var length) || length < 0 || length > NetworkConfig.MAX_MESSAGE_SIZE)
            throw new InvalidDataException("Invalid EJLive frame length.");

        var body = ReadExact(stream, length);
        if (sessionKey is { Length: > 0 } && body.Length > 0)
            body = SecurityHelper.DecryptAES(body, sessionKey);

        return new EJMessage
        {
            Type = type,
            Payload = body,
            Text = WireEncoding.GetString(body),
            ReceivedAtUtc = DateTime.UtcNow
        };
    }

    public static string BuildDelimitedMessage(string messageType, params string[] parts) => Protocol.BuildMessage(messageType, parts);
    public static string[] ParseDelimitedMessage(string message) => Protocol.ParseMessage(message);

    private static string ReadHeader(Stream stream)
    {
        var buffer = new List<byte>(64);
        while (true)
        {
            var next = stream.ReadByte();
            if (next < 0)
                throw new EndOfStreamException("Connection closed while reading EJLive frame header.");
            if (next == '\n')
                break;
            buffer.Add((byte)next);
            if (buffer.Count > 128)
                throw new InvalidDataException("EJLive frame header is too large.");
        }
        return WireEncoding.GetString(buffer.ToArray());
    }

    private static byte[] ReadExact(Stream stream, int length)
    {
        var buffer = new byte[length];
        var offset = 0;
        while (offset < length)
        {
            var read = stream.Read(buffer, offset, length - offset);
            if (read == 0)
                throw new EndOfStreamException("Connection closed while reading EJLive frame body.");
            offset += read;
        }
        return buffer;
    }
}

public sealed class RemoteCommandEnvelope
{
    public string CommandId { get; set; } = Guid.NewGuid().ToString("N");
    public string CommandType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public bool RequiresConfirmation { get; set; }
    public string IssuedAtUtc { get; set; } = DateTime.UtcNow.ToString("O");
    public string Nonce { get; set; } = Guid.NewGuid().ToString("N");
    public string Signature { get; set; } = string.Empty;
    public string SignatureVersion { get; set; } = CommandEnvelopeAuthenticator.SignatureVersion;
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public DateTime ExpiryUtc { get; set; } = DateTime.UtcNow.AddMinutes(5);
    public string IssuerRole { get; set; } = "Support";
    public string IssuerId { get; set; } = string.Empty;
    public CommandRiskLevel RiskLevel { get; set; } = CommandRiskLevel.Low;
    public bool SignatureVerified { get; private set; }
    public string SignatureFailureReason { get; private set; } = string.Empty;

    public string ToWireText()
    {
        if (string.IsNullOrWhiteSpace(CommandId))
            CommandId = Guid.NewGuid().ToString("N");
        if (string.IsNullOrWhiteSpace(IssuedAtUtc))
            IssuedAtUtc = DateTime.UtcNow.ToString("O");
        if (string.IsNullOrWhiteSpace(Nonce))
            Nonce = Guid.NewGuid().ToString("N");

        SignatureVersion = CommandEnvelopeAuthenticator.SignatureVersion;
        var payloadBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(Payload ?? string.Empty));
        var signingPayload = CommandEnvelopeAuthenticator.BuildSigningPayload(
            CommandId,
            CommandType,
            RequiresConfirmation,
            payloadBase64,
            IssuedAtUtc,
            Nonce);
        Signature = CommandEnvelopeAuthenticator.SignPayload(signingPayload);

        return string.Join("|", new[]
        {
            CommandId,
            CommandType,
            RequiresConfirmation.ToString(),
            payloadBase64,
            IssuedAtUtc,
            Nonce,
            Signature,
            SignatureVersion
        });
    }

    public static bool TryParse(string wireText, out RemoteCommandEnvelope command)
    {
        command = new RemoteCommandEnvelope();
        var parts = (wireText ?? string.Empty).Split('|');
        if (parts.Length < 3)
            return false;

        command.CommandId = parts[0];
        command.CommandType = parts[1];
        command.RequiresConfirmation = bool.TryParse(parts[2], out var requiresConfirmation) && requiresConfirmation;
        var payloadBase64 = string.Empty;
        if (parts.Length >= 4)
        {
            payloadBase64 = parts[3];
            try
            {
                command.Payload = Encoding.UTF8.GetString(Convert.FromBase64String(payloadBase64));
            }
            catch (FormatException)
            {
                command.Payload = payloadBase64;
            }
        }

        if (parts.Length >= 8)
        {
            command.IssuedAtUtc = parts[4];
            command.Nonce = parts[5];
            command.Signature = parts[6];
            command.SignatureVersion = parts[7];

            if (!string.Equals(command.SignatureVersion, CommandEnvelopeAuthenticator.SignatureVersion, StringComparison.OrdinalIgnoreCase))
            {
                command.SignatureFailureReason = "Unsupported signature version.";
                return false;
            }

            if (!CommandEnvelopeAuthenticator.IsFresh(command.IssuedAtUtc, out var freshnessReason))
            {
                command.SignatureFailureReason = freshnessReason;
                return false;
            }

            var signingPayload = CommandEnvelopeAuthenticator.BuildSigningPayload(
                command.CommandId,
                command.CommandType,
                command.RequiresConfirmation,
                payloadBase64,
                command.IssuedAtUtc,
                command.Nonce);
            if (!CommandEnvelopeAuthenticator.VerifyPayload(signingPayload, command.Signature, out var verifyReason))
            {
                command.SignatureFailureReason = verifyReason;
                return false;
            }

            command.SignatureVerified = true;
            return !string.IsNullOrWhiteSpace(command.CommandId) && !string.IsNullOrWhiteSpace(command.CommandType);
        }

        if (!CommandEnvelopeAuthenticator.AllowUnsignedCommands)
        {
            command.SignatureFailureReason = "Unsigned command rejected by policy.";
            return false;
        }

        command.SignatureVerified = false;
        command.SignatureFailureReason = "Unsigned command accepted by explicit compatibility policy.";
        return !string.IsNullOrWhiteSpace(command.CommandId) && !string.IsNullOrWhiteSpace(command.CommandType);
    }
}
