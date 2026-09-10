using System.Collections.Concurrent;

namespace EJLive.Core.Engine;

/// <summary>
/// Registry that maps ATM vendor types to their corresponding EJ transaction parser implementations.
/// Thread-safe and supports runtime registration of additional parsers.
/// </summary>
public sealed class EjParserRegistry
{
    private readonly ConcurrentDictionary<string, IEjTransactionParser> _parsers = new(StringComparer.OrdinalIgnoreCase);
    private readonly IEjTransactionParser _fallback;

    public EjParserRegistry(IEjTransactionParser? fallback = null)
    {
        _fallback = fallback ?? new PreservingFallbackEjTransactionParser();
    }

    /// <summary>
    /// Gets the singleton default registry with all known vendor parsers pre-registered.
    /// </summary>
    public static EjParserRegistry Default { get; } = CreateDefault();

    /// <summary>
    /// Registers a parser for the specified vendor type.
    /// </summary>
    /// <param name="vendorType">The vendor identifier (e.g., NCR, GRG, Wincor, Diebold, Hyosung).</param>
    /// <param name="parser">The parser instance to register.</param>
    public void Register(string vendorType, IEjTransactionParser parser)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(vendorType);
        ArgumentNullException.ThrowIfNull(parser);
        _parsers[vendorType] = parser;
    }

    /// <summary>
    /// Retrieves the parser for a given vendor type.
    /// </summary>
    /// <param name="vendorType">The vendor identifier.</param>
    /// <returns>The registered parser, or null if no parser is registered for the vendor.</returns>
    public IEjTransactionParser? GetParser(string vendorType)
    {
        if (string.IsNullOrWhiteSpace(vendorType))
            return null;
        _parsers.TryGetValue(vendorType, out var parser);
        return parser;
    }

    /// <summary>Resolves a vendor parser or the lossless fallback.</summary>
    public IEjTransactionParser Resolve(string? vendorType) =>
        GetParser(vendorType ?? string.Empty) ?? _fallback;

    /// <summary>
    /// Retrieves the parser for a given vendor type, throwing if not found.
    /// </summary>
    /// <param name="vendorType">The vendor identifier.</param>
    /// <returns>The registered parser.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no parser is registered for the vendor.</exception>
    public IEjTransactionParser GetRequiredParser(string vendorType)
    {
        return GetParser(vendorType)
            ?? throw new KeyNotFoundException($"No EJ parser registered for vendor type '{vendorType}'.");
    }

    /// <summary>
    /// Gets a read-only snapshot of all registered vendor types.
    /// </summary>
    public IReadOnlyCollection<string> RegisteredVendors => _parsers.Keys.ToArray();

    /// <summary>
    /// Returns true if a parser is registered for the specified vendor type.
    /// </summary>
    public bool IsRegistered(string vendorType) =>
        !string.IsNullOrWhiteSpace(vendorType) && _parsers.ContainsKey(vendorType);

    private static EjParserRegistry CreateDefault()
    {
        var registry = new EjParserRegistry();
        registry.Register("NCR", new NcrEjTransactionParser());
        registry.Register("GRG", new GrgEjTransactionParser());
        registry.Register("Wincor", new WincorEjTransactionParser());
        registry.Register("Diebold", new DieboldEjTransactionParser());
        registry.Register("Hyosung", new HyosungEjTransactionParser());
        registry.Register("Cashway", new CashwayEjTransactionParser());
        return registry;
    }
}
