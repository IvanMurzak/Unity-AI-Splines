# AGENTS.md — Unity-AI-Splines

Unity package `com.ivanmurzak.unity.mcp.splines` — makes **Unity Splines** (`com.unity.splines`) available to
Unity-MCP's RQL tools (`rql-query` / `rql-mutate`; no extension MCP tools): the `splines`
source (containers, `CreateContainer`), SplineContainer methods (add / insert / remove knots and splines,
evaluate), writable `splines[i].knots[j]` views (position, tangents, rotation, tangent mode, closed), and
Unity.Mathematics literals. An extension of
[AI Game Developer](https://github.com/IvanMurzak/Unity-MCP) (`com.ivanmurzak.unity.mcp`), generated from
`Unity-AI-Tools-Template`; the user-facing tool list is in `README.md`.

## Layout

- `Unity-Package/` — Unity project hosting the package; only `Packages/com.ivanmurzak.unity.mcp.splines/`
  ships.
  - `package.json` — the version source of truth and the dependency pins.
  - `Editor/Scripts/Rql/` — the RQL module (`SplinesModule`: source, adapters, guide), one adapter per file
    (`<Namespace_Type>_Adapter.cs`), the `SplinesApi` source object and the `SplineView` / `KnotView` views.
  - `Runtime/` — runtime assembly (currently empty).
  - `Tests/Editor/Rql/` (EditMode, `SplinesTestBase` + one file per adapter / view / source) and
    `Tests/Runtime/` (PlayMode).
- `Unity-Tests/<unity-version>/` — one test project per supported Unity version, consuming the package via
  `file:`. A new version must also be added to `test_pull_request.yml` and `release.yml` (and
  `open-all-projects-*`).
- `Installer/` — Unity project that exports the `.unitypackage` installer
  (`Assets/com.IvanMurzak/<name> Installer/`).
- `commands/` — PowerShell helpers (below) and `check-versions.py` (synced in by the release cascade — do not
  hand-edit; CI runs it as the `version-consistency` gate).
- `.github/workflows/` — `test_pull_request.yml` (PR test matrix), `test_unity_plugin.yml` (reusable test
  job), `release.yml`, `bump_version.yml`.
- `docs/` — user docs: `Deploy-{OpenUPM,GitHub,npmjs}.md`, `Manual-Package-Rename.md`, `openupm-signing.md`,
  `img/`, `promo/`.

## Commands (PowerShell, from the repo root)

```powershell
./commands/bump-version.ps1 -NewVersion "x.y.z"   # package.json + Installer.cs in lock-step; -WhatIf to preview
./commands/get-version.ps1
./commands/update-ai-game-developer.ps1           # bump the com.ivanmurzak.unity.mcp pin; -WhatIf to preview
./commands/open-all-projects-windows.ps1          # open all Unity projects (or -unix.sh)
```

Tests run inside the Unity Editor (NUnit + `[UnityTest]`); CI runs them with `game-ci/unity-test-runner` in
EditMode / PlayMode / Standalone for every Unity version. `release.yml` releases on push to `main` only when
the `package.json` version has no tag yet (installer, signed UPM tarball for OpenUPM, test matrix, tag, GitHub
Release); dispatch `bump_version.yml` to bump in CI. Under `pull_request_target`, tests need the
maintainer-applied `ci-ok` label and abort if the PR touches `.github/workflows/`.

## Critical invariants

- **RQL surface.** Everything is registered by `SplinesModule` (`IRqlModule`, Order 100, found through
  TypeCache) and documented in its `AddGuide("Splines", …)` text, which is rendered into the `unity-rql` skill:
  keep the guide examples runnable (`SplinesModuleTests.EveryGuideExampleRuns` executes them in order).
- **Undo.** Calls on a SplineContainer are recorded by the engine (the container is the owner). Writes through
  `SplineView` / `KnotView` have a plain owner, so their setters record the container themselves (`SplineEdit`).
- **Spline data model.** A `SplineContainer` holds one or more `Spline`s; each `Spline` is a list of
  `BezierKnot`s (Position, TangentIn, TangentOut, Rotation) plus a `Closed` flag and per-knot `TangentMode`.
  Edit knots through `Spline`'s `Add` / `Insert` / `RemoveAt` / `SetTangentMode` and the indexer, never by raw
  struct mutation, so the spline rebuilds its cached curve.
- Every C# file starts with the copyright box header and `#nullable enable`.
