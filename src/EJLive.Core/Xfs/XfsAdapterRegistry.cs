using System;
using System.Collections.Generic;
using System.Linq;
using EJLive.Core.Xfs.Adapters;

namespace EJLive.Core.Xfs
{
    public sealed class XfsAdapterRegistry
    {
        private readonly List<IXfsVendorAdapter> _adapters;

        public XfsAdapterRegistry()
        {
            _adapters = new List<IXfsVendorAdapter>
            {
                new Adapters.NcrXfsEventAdapter(),
                new Adapters.DieboldMdsAdapter(),
                new Adapters.GrgJournalAdapter(),
                new Adapters.CardReaderTraceAdapter(),
                new Adapters.HostMessageInAdapter(),
                new Adapters.HostMessageOutAdapter(),
                new Adapters.OoxfsRuntimeAdapter(),
                new Adapters.DebugTraceAdapter()
            };
        }

        public IReadOnlyList<IXfsVendorAdapter> Adapters => _adapters;

        public IXfsVendorAdapter? ResolveAdapter(IEnumerable<string> lines)
        {
            if (lines == null)
                return null;

            var sample = lines.Where(l => !string.IsNullOrWhiteSpace(l)).Take(50).ToArray();
            foreach (var adapter in _adapters)
            {
                if (sample.Any(adapter.CanHandle))
                    return adapter;
            }

            return null;
        }

        public IReadOnlyList<XfsNormalizedEvent> Parse(IEnumerable<string> lines)
        {
            var source = lines == null ? Array.Empty<string>() : lines.ToArray();
            var adapter = ResolveAdapter(source);
            return adapter != null ? adapter.ParseLines(source) : Array.Empty<XfsNormalizedEvent>();
        }
    }
}
