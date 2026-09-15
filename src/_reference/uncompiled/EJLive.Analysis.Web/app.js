// EJLive Smart Analysis — minimal web client for the SS-27 REST host.
// All routes are relative so the static host can sit behind any reverse
// proxy or alongside the SmartAnalysisHost on the same origin.

const $ = (id) => document.getElementById(id);

const state = {
  base: window.location.origin,
  lastReport: null,
  lastFindings: [],
};

document.addEventListener('DOMContentLoaded', () => {
  $('analyze-text').addEventListener('click', analyzeText);
  $('analyze-file').addEventListener('click', analyzeFile);
  $('load-sample').addEventListener('click', loadSample);
  $('clear').addEventListener('click', clearAll);
  $('sort-severity').addEventListener('click', () => renderFindings(sortBy(state.lastFindings, 'severity')));
  $('sort-category').addEventListener('click', () => renderFindings(sortBy(state.lastFindings, 'category')));
  refreshHealth();
  refreshRecent();
  setInterval(refreshHealth, 30000);
  setInterval(refreshRecent, 30000);
});

async function refreshHealth() {
  try {
    const r = await fetch(`${state.base}/api/health`);
    if (!r.ok) throw new Error(`HTTP ${r.status}`);
    const j = await r.json();
    $('health-pill').textContent =
      `health: ok · port ${j.port} · recent ${j.recentUploads} · ${Math.round(j.uptimeSeconds)}s`;
    $('health-pill').style.color = '#5f27cd';
  } catch (e) {
    $('health-pill').textContent = `health: offline (${e.message})`;
    $('health-pill').style.color = '#ee5253';
  }
}

async function refreshRecent() {
  try {
    const r = await fetch(`${state.base}/api/analysis/recent?n=10`);
    if (!r.ok) return;
    const list = await r.json();
    const tbody = $('recent-table').querySelector('tbody');
    tbody.innerHTML = '';
    list.forEach((row) => {
      const tr = document.createElement('tr');
      tr.innerHTML =
        `<td>${row.traceId ?? ''}</td>` +
        `<td>${row.sourceLabel ?? ''}</td>` +
        `<td>${row.vendorHint ?? ''}</td>` +
        `<td>${row.lineCount ?? 0}</td>` +
        `<td>${row.criticalCount ?? 0} / ${row.warningCount ?? 0} / ${row.infoCount ?? 0}</td>` +
        `<td>${(row.analyzedAtUtc ?? '').replace('T', ' ').replace('Z', '')}</td>`;
      tbody.appendChild(tr);
    });
  } catch (_) { /* ignore */ }
}

async function analyzeText() {
  const payload = $('payload').value.trim();
  if (!payload) {
    alert('Paste a journal payload first.');
    return;
  }
  const headers = { 'Content-Type': 'text/plain' };
  if ($('vendor').value.trim()) headers['X-Vendor'] = $('vendor').value.trim();
  headers['X-Source'] = $('source').value.trim() || 'web-ui';

  const r = await fetch(`${state.base}/api/analysis/upload`, {
    method: 'POST',
    headers,
    body: payload,
  });
  if (!r.ok) {
    alert(`Upload failed: HTTP ${r.status}`);
    return;
  }
  const report = await r.json();
  ingestReport(report);
  refreshRecent();
}

async function analyzeFile() {
  const fileInput = $('file');
  if (!fileInput.files || fileInput.files.length === 0) {
    alert('Pick a file first.');
    return;
  }
  const form = new FormData();
  form.append('file', fileInput.files[0]);
  const r = await fetch(`${state.base}/api/analysis/files`, { method: 'POST', body: form });
  if (!r.ok) {
    alert(`Upload failed: HTTP ${r.status}`);
    return;
  }
  const summary = await r.json();
  $('source').value = summary.savedPath ?? $('source').value;
  if (summary.traceId) {
    const r2 = await fetch(`${state.base}/api/analysis/by-trace/${summary.traceId}`);
    if (r2.ok) ingestReport(await r2.json());
  }
  refreshRecent();
}

function loadSample() {
  $('payload').value = [
    '[2025-09-14 09:14:11] NCR SDC LINK FAULT: M-146 LOST on dispenser 3a handler.',
    '[2025-09-14 09:14:13] GRG CIM RETRACT: cash deposit retract, bin full.',
    '[2025-09-14 09:14:21] NCR PRINTER PART REPLACE: printhead fault detected.',
    '[2025-09-14 09:14:31] WINCOR WOSA/XFS SP ERROR: CDM CASH UNIT EMPTY (CS2)',
    '[2025-09-14 09:14:45] DEPOSIT AMT=850.00 SAR processed.',
    '[2025-09-14 09:14:55] WITHDRAWAL AMT=300.00 SAR completed.',
    '[2025-09-14 09:15:02] GRG TAKE CASH TIMEOUT on dispense.',
    '[2025-09-14 09:15:11] HYOSUNG HCDM DISPENSE FAULT: retract to reject bin.',
  ].join('\n');
  $('vendor').value = '';
  $('source').value = 'sample-' + new Date().toISOString().slice(0, 19);
}

function clearAll() {
  $('payload').value = '';
  $('file').value = '';
  ['summary-card', 'value-card', 'findings-card'].forEach((id) => ($(id).hidden = true));
  state.lastReport = null;
  state.lastFindings = [];
}

function ingestReport(report) {
  state.lastReport = report;
  state.lastFindings = report.findings || [];
  renderSummary(report);
  renderValue(report.value);
  renderFindings(state.lastFindings);
  ['summary-card', 'value-card', 'findings-card'].forEach((id) => ($(id).hidden = false));
}

function renderSummary(report) {
  $('m-critical').textContent = report.criticalCount ?? 0;
  $('m-warning').textContent = report.warningCount ?? 0;
  $('m-info').textContent = report.infoCount ?? 0;
  $('m-lines').textContent = report.lineCount ?? 0;
  $('trace-id').textContent =
    `trace: ${report.traceId} · vendor: ${report.vendorHint || 'auto'} · ` +
    `${report.analyzedAtUtc?.replace('T', ' ').replace('Z', '') ?? '—'}`;
}

function renderValue(value) {
  if (!value) return;
  $('v-source').textContent = value.source ?? '—';
  $('v-withdrawals').textContent = value.withdrawals ?? 0;
  $('v-deposits').textContent = value.deposits ?? 0;
  $('v-dispense').textContent = (value.dispenseAmount ?? 0).toFixed(2);
  $('v-deposit').textContent = (value.depositAmount ?? 0).toFixed(2);
  $('v-errors').textContent = value.errors ?? 0;
  $('v-cards').textContent = value.cardRetained ?? 0;
  $('v-paper').textContent = value.paperWarnings ?? 0;

  const cassetteBody = $('cassette-table').querySelector('tbody');
  cassetteBody.innerHTML = '';
  Object.entries(value.cassetteMoves ?? {}).forEach(([slot, notes]) => {
    const tr = document.createElement('tr');
    tr.innerHTML = `<td>CS${slot}</td><td>${notes}</td>`;
    cassetteBody.appendChild(tr);
  });

  const hourlyBody = $('hourly-table').querySelector('tbody');
  hourlyBody.innerHTML = '';
  const max = Math.max(1, ...Object.values(value.hourlyDensity ?? {}));
  Object.entries(value.hourlyDensity ?? {}).forEach(([h, lines]) => {
    const tr = document.createElement('tr');
    const bar = `<span class="hour-bar" style="width:${Math.round((lines / max) * 80)}px"></span>`;
    tr.innerHTML = `<td>${h.padStart(2, '0')}:00</td><td>${bar}${lines}</td>`;
    hourlyBody.appendChild(tr);
  });
}

function renderFindings(findings) {
  const tbody = $('findings-table').querySelector('tbody');
  tbody.innerHTML = '';
  findings.forEach((f) => {
    const tr = document.createElement('tr');
    tr.innerHTML =
      `<td class="severity-${f.severity}">${f.severity}</td>` +
      `<td>${f.category}</td>` +
      `<td>${f.code}</td>` +
      `<td>${f.vendor}</td>` +
      `<td>${f.recommendedAction}</td>` +
      `<td>${f.sourceLabel}</td>`;
    tbody.appendChild(tr);
  });
}

function sortBy(findings, mode) {
  const rank = { Critical: 0, Warning: 1, Info: 2 };
  const list = findings.slice();
  if (mode === 'severity') {
    list.sort((a, b) => (rank[a.severity] ?? 99) - (rank[b.severity] ?? 99));
  } else {
    list.sort((a, b) => (a.category || '').localeCompare(b.category || ''));
  }
  return list;
}
