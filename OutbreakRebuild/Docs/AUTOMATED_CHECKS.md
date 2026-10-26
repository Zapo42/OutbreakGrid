# Automated check setup

Gameplay baseline: Foundation 130. This maintenance update changes documentation and test tooling only. Unity remains pinned to **6000.6.3f1**; Foundation 82 is the last user-reported working native Unity checkpoint.

## Recommended: .NET 8 SDK and Python 3

From `OutbreakRebuild`:

```bash
python3 Tools/run_checks.py
```

The runner prefers a .NET SDK on PATH. Use an explicit executable when it is installed elsewhere:

```bash
python3 Tools/run_checks.py --dotnet /path/to/dotnet --output "/path/to/check results"
```

On Windows, use `python` if that is how Python is installed and pass the path to `dotnet.exe` when needed. These are host checks, not a Windows game build.

For this cloud workspace:

```bash
DOTNET_CLI_HOME=/workspace/toolchains/dotnet-home python3 Tools/run_checks.py --dotnet /workspace/toolchains/dotnet/dotnet --output /tmp/outbreak-check-results
```

Run the script by absolute path from any working directory if preferred. The script locates the project relative to itself; quote executable and output paths containing spaces. Reports go to a printed temporary directory by default and remain available after completion. `--output` chooses a persistent destination. Use a fresh directory to keep evidence separate. The default does not overwrite checked-in `Examples` files; explicitly choosing `Examples` requests that output location.

## What runs

Checks execute sequentially, stopping on failure:

1. Unity target/module configuration validation.
2. `Tests/FoundationChecks.csproj`: production core/persistence, current acceptance cases, saved fixtures, continuation and the baseline movement benchmark ladder.
3. `Tests/WindowsDialogChecks.csproj`: compilation of the actual Windows dialog branch and pointer-sized native record layout; it does not open Windows dialogs.
4. `Tests/UiChecks.csproj` with Legacy input symbols: production UI handlers against narrow Unity API doubles.
5. The same UI project with NewOnly input symbols: incompatible-input diagnostic behavior.

`FOUNDATION_CHECKS.txt` records the core result and appended Windows/UI evidence. Core example map/save files are also generated in the chosen output directory. Console output records commands and results. Capture it when a complete execution log is needed. A nonzero exit indicates failure; do not treat partially written reports as a passing run.

The C# project manifests are the source of truth for source inclusion. Do not maintain a second hand-written list in the runner. The test projects share build intermediates, so avoid concurrent builds in one checkout. Use separate worktrees for concurrent development. `CityPerformance.csproj` is a separate focused performance tool, not part of this acceptance runner; movement-only timings do not establish full-game scale or native frame rates.

## Mono compatibility

If Mono is available, explicitly select it:

```bash
python3 Tools/run_checks.py --compiler /path/to/mcs --mono /path/to/mono --output /path/to/results
```

Do not combine Mono selection with `--dotnet`. The Mono path expands the current project manifests rather than using the old incomplete source lists. Backend support and checks actually run for each increment are recorded separately in the build log; successful .NET execution does not prove Mono execution.

## Limits and release evidence

A passing result establishes the stated host checks only. Native Unity import, package resolution, rendering/layout, input backend, operating-system dialogs and Windows player smoke tests require separate evidence. No Unity or dependency upgrade is performed by this runner. The editor’s **Outbreak > Rebuild > Build Windows player** action remains interactive and was not exercised by this maintenance task.

See [Build validation](BUILD_VALIDATION.md), [team assignments](../../PLANS.md), and [verification log](../../harness/build-log.md). `RESTORE42` preserves the pre-team Foundation 130 project.
