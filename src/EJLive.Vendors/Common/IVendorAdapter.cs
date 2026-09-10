using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EJLive.Vendors.Common
{
    /// <summary>Root vendor adapter — identifies vendor and provides all sub-adapters.</summary>
    public interface IAtmVendorAdapter
    {
        string Vendor { get; }
        string[] SupportedModels { get; }
        IJournalParser JournalParser { get; }
        ILogParser LogParser { get; }
        INdcComAdapter? NdcComAdapter { get; }
        IXfsSpAdapter? XfsSpAdapter { get; }
        IVendorErrorDictionary ErrorDictionary { get; }
        ICashCassetteParser? CashCassetteParser { get; }
    }

    /// <summary>Parses ATM electronic journal files into EjTransaction records.</summary>
    public interface IJournalParser
    {
        Task<List<object>> ParseAsync(string filePath);
        string VendorType { get; }
    }

    /// <summary>Parses real ATM log files (TRACE, COM, DEV, etc).</summary>
    public interface ILogParser
    {
        Task<List<object>> ParseAsync(string filePath);
        List<string> SupportedExtensions { get; }
    }

    /// <summary>Adapter for NDC/COM switch communication analysis.</summary>
    public interface INdcComAdapter
    {
        string Protocol { get; }
        object? DecodeMessage(string rawLine);
        Task<bool> IsSupported(string atmId);
    }

    /// <summary>Adapter for XFS/SP device state analysis.</summary>
    public interface IXfsSpAdapter
    {
        string XfsVersion { get; }
        object? NormalizeEvent(string rawLine);
        List<string> SupportedDeviceClasses { get; }
    }

    /// <summary>Vendor error code dictionary — maps raw codes to normalized meanings.</summary>
    public interface IVendorErrorDictionary
    {
        string? Lookup(string code);
        Dictionary<string, string> AllCodes { get; }
    }

    /// <summary>Parses cash/cassette counters from device logs or EJ.</summary>
    public interface ICashCassetteParser
    {
        object? ParseCassetteStatus(string rawLine);
        int CassetteCount { get; }
    }
}
