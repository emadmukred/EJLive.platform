# EJLive Smart Analysis — Web Frontend

Static HTML / CSS / JavaScript client for the
**`SmartAnalysisHost`** REST bridge shipped in Wave 5 / SS-27.

The page calls the host on the **same origin** so it can be served from
any static file host (nginx, IIS, Kestrel static, `python -m http.server`)
without needing a build step.

## Endpoints consumed

| Method | Path                              | Purpose                                           |
|-------:|-----------------------------------|---------------------------------------------------|
| GET    | `/api/health`                     | Liveness + recent-upload count                    |
| POST   | `/api/analysis/upload`            | Analyze raw text payload (sets `X-Vendor`, `X-Source`, `X-Trace` headers) |
| POST   | `/api/analysis/files`             | Multipart upload of a journal file                |
| GET    | `/api/analysis/recent?n=10`       | List the most recent analyses                     |
| GET    | `/api/analysis/by-trace/{id}`     | Re-fetch a single analysis report                |
| GET    | `/api/analysis/by-source/{label}` | Latest analysis for a given source label          |
| GET    | `/api/analysis/categories`        | Aggregated category counts across recent uploads  |

## Local development

```bash
# 1. Start the SmartAnalysisHost on http://127.0.0.1:8765
cd src/EJLive.Application
dotnet run --project EJLive.Application.csproj
# (sample boot helper: app.StartSmartAnalysis("http://127.0.0.1:8765/", "./uploads"))

# 2. Serve the static page from any HTTP origin. Easiest:
cd src/EJLive.Analysis.Web
python3 -m http.server 8080
# Then open http://127.0.0.1:8080/ and point the API origin in app.js if needed
```

Because the client uses `window.location.origin` for the API base, the
two servers must be reachable from the same browser origin (a
reverse-proxy in front of both is the recommended production setup).

## Files

- `index.html` — markup, four primary cards (upload, summary, value,
  findings) + recent uploads table.
- `styles.css` — light-theme stylesheet, mirror of the canonical
  `EJLive.Core.UI.LightUiTheme` palette tokens (same hex values).
- `app.js` — `fetch`-based REST client, no framework, no build step.
  Loads sample text, analyses text/file uploads, re-sorts findings
  client-side after the server returns them.
