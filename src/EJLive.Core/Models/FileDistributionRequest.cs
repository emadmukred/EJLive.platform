namespace EJLive.Core.Models;

/// <summary>
/// Represents a request to distribute a file from the server to an ATM.
/// </summary>
public sealed record FileDistributionRequest
{
    /// <summary>Unique identifier for the distribution request.</summary>
    public required Guid RequestId { get; init; }

    /// <summary>Identifier of the target ATM.</summary>
    public required string AtmId { get; init; }

    /// <summary>Source path on the server.</summary>
    public required string SourcePath { get; init; }

    /// <summary>Desired destination path on the ATM.</summary>
    public required string DestinationPath { get; init; }

    /// <summary>Expected SHA-256 hash of the file content.</summary>
    public required string ExpectedSha256 { get; init; }

    /// <summary>Allowed destination folder that the file must reside within.</summary>
    public required string AllowedDestinationFolder { get; init; }
}
