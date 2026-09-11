# PERMISSIONS ledger (generated -- do not hand-edit)

Source: `tools/inventory/ejlive_inventory.py`. Regenerate with `python3 tools/inventory/ejlive_inventory.py`.

| enumerable / gate / attribute | definition | map | file |
|---|---|---|---|
| gate:CanParse | public bool CanParse(EjParseContext context) => true; | active | src/EJLive.Core/Journal/JournalContracts.cs |

Authorization is enforced at three chokepoints: intake (message -> risk tier), executor (role x risk matrix), UI (control binding mirrors the matrix, never the inverse). See SS9.
