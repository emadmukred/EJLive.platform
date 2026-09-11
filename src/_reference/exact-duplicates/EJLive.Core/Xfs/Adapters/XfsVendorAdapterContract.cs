using System.Collections.Generic;

namespace EJLive.Core.Xfs.Adapters
{
    public interface IXfsVendorAdapter
    {
        XfsVendor Vendor { get; }
        bool CanHandle(string line);
        IReadOnlyList<XfsNormalizedEvent> ParseLines(IEnumerable<string> lines);
    }
}
