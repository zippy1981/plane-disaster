# Working in this repo

PlaneDisaster.NET is a Windows Forms front end for Access (Jet) and SQLite databases, written in C#. It ships for .NET Framework 2.0 and also builds for .NET Framework 4.8.1 and .NET 11. The solution is `PlaneDisaster.NET.sln`.

This file is the OpenCode (and other agent) counterpart to [CLAUDE.md](CLAUDE.md). The two files carry the same rules. When a rule changes, change both files in the same commit.

## OpenCode in this repo

- **Rules:** OpenCode reads this file as project rules. `opencode.json` adds `.claude/skills` to the skill paths, so the Claude Code skills load as they are. Do not copy a skill into `.opencode/skills`.
- **Agents:** `.opencode/agent/<name>.md` mirrors every `.claude/agents/<name>.md` playbook. The Claude playbook is the source of truth; edit it, then update the wrapper's description and permissions.
- **Models:** OpenCode subagents inherit the session model. To keep the cheap-model split in [.claude/agents/README.md](.claude/agents/README.md), add `model: <provider>/<model-id>` to a wrapper. Bare Claude names (`haiku`, `sonnet`, `opus`) are not OpenCode model IDs.

## Building

- The projects are SDK-style and multi-target `net20` (what ships), `net481` and `net11.0-windows` (`Directory.Build.props`). Platforms are x86 and x64; there is no AnyCPU. Output goes to `bin\<platform>\<configuration>\<tfm>\`.
- Build one framework at a time. Restore with `dotnet restore PlaneDisaster.NET.sln -p:Platform=x64`. Build .NET Framework with full MSBuild: `msbuild PlaneDisaster.NET.sln -p:Configuration=Release -p:Platform=x64 -p:TargetFramework=net20`. `dotnet build` can't embed the forms' non-string `.resx` resources for .NET Framework. Build .NET with `dotnet build PlaneDisaster.NET.sln -c Release -p:Platform=x64 -p:TargetFramework=net11.0-windows`.
- CI (`.github/workflows/build.yml`, `windows-latest`) runs net20, net481 and net11.0-windows Release, each for x86 and x64, and adds net20 Debug on pushes to `master`. Those x86 legs test Jet 4.0 and the x64 legs the Access Database Engine 2016 MSI; the `Access drivers` job runs the net481 tests against the 32 bit engine and the Microsoft 365 Click-to-Run engine (x86 and x64). Every leg sets `PLANEDISASTER_EXPECT_ACCESS_FORMATS` to the formats its drivers must open, so a test that finds no driver for one of them fails instead of being ignored. It caches `~/.nuget/packages`. A `mono` job builds with the latest Mono (Debian unstable, in a container) through `build/mono/build.sh`, which compiles the same sources with `mcs` because Mono can't load SDK-style projects. It runs only the tests that need no Access driver (SQLite, the tokenizer, and the driver choice in `TestAccessDrivers`), since Access needs Windows. A new source folder or resource must be added to that script too; the `Mono source list` job (`build/mono/check-sources.sh`) fails when its file list differs from the projects' Compile items. The .NET Framework tests use NUnit 2.6 (the `NUnit` 2.6.2 package, run by nunit-console). The .NET tests use NUnit 3 via `dotnet test`, on x64 only. That leg also measures code coverage (`--collect "Code Coverage"`, Cobertura); ReportGenerator writes the summary to the job summary and the HTML report to the `coverage-report` artifact. A change that touches only Markdown or agent config skips the Windows build; the `CI result` job still reports.
- Every framework uses the `System.Data.SQLite.Core` 1.0.119 package: a managed assembly plus `x86\SQLite.Interop.dll` and `x64\SQLite.Interop.dll`, which the MSI and the portable zip ship. .NET also uses the `System.Data.OleDb`/`Odbc` and `System.Configuration.ConfigurationManager` packages. Packages come from NuGet; nothing is committed under `packages/`.
- `PlaneDisaster.NET Setup` is a WiX 5 SDK project, built after the solution with `dotnet build "PlaneDisaster.NET Setup" -c Release -p:Platform=<x86|x64>`. CI builds the MSIs on the Release legs. WiX 6 and later need the OSMF EULA accepted, so stay on 5 unless the user decides otherwise.
- Releases: bump `AssemblyVersion` in every `AssemblyInfo.cs`, merge, then push a `v<version>` tag. `release.yml` builds and publishes the MSIs and portable zips, and fails if the tag and `AssemblyInfo.cs` disagree.
- On a host without Visual Studio, push a branch and let CI build the Windows legs. Linux work (the Mono build, a .NET SDK container, the experiments) can run on a Linux Docker engine, such as a WSL distro's: follow the `host-capabilities` skill, and clean up with `safe-docker-cleanup`. The `host-doctor` playbook diagnoses a host that can't.

## Code style

- Prefer an extension method to a static helper that takes the object as its first argument: `type.ToSQLiteColumnType()`, not `SQLiteColumnType(type)`. Put it in a static `<Type>Extensions` class. .NET Framework 2.0 has no `ExtensionAttribute`, so `PlaneDisaster.Dba/TypeExtensions.cs` declares an internal one under `#if NET20`; an assembly that defines its own extension methods needs the same.
- Prefer parameterized tests. When a test checks the same behavior for several inputs, give each input its own case with `[TestCase]`, or `[TestCaseSource]` when the values aren't compile-time constants, instead of asserting on all of them in one test. Both work in NUnit 2.6 and NUnit 3.
- `UnitTests/TestData/` holds Access databases from public sources for the smoke tests in `TestAccessSampleFiles.cs`; the project copies them to the output directory. Add a file only if it is public domain or freely redistributable, and record its source, licence and SHA-256 in that folder's `README.md`.

## Markdown

Every `.md` file must pass markdownlint (`.github/workflows/markdown-lint.yml`). Follow the `markdown-lint` skill, or the `markdown-reviewer` and `markdown-fixer` playbooks. Markdown, YAML and JSON are LF (`.gitattributes`).

## GitHub workflow

- The default branch is `master`. Branch from `origin/master` in a worktree (`git worktree add ../wt-<name> -b <branch> origin/master`). The main checkout belongs to the user.
- Commits end with the attribution trailers the session gives you. PR bodies end with the attribution line the session gives you.
- Wait on CI with `gh pr checks <n> --watch` instead of polling. On failure, read only the failed step: `gh run view <id> --log-failed`.
- `master` requires the `CI result` and `lint` checks. Once the user has asked for a merge, queue it with `gh pr merge <n> --squash --auto --delete-branch` and stop: GitHub merges when the checks pass, so there is nothing to watch. Never use `--admin`.
- Running `gh pr merge` in a worktree prints `fatal: 'master' is already used by worktree` after merging, because it tries to switch the local branch. The merge still happened; confirm with `gh pr view <n> --json state`.
- Action versions are pinned to commit SHAs with the tag in a comment. Dependabot updates them weekly.
