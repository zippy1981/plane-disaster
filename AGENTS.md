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
- CI (`.github/workflows/build.yml`, `windows-latest`) runs net20 Debug and Release plus net481 and net11.0-windows Release, each for x86 and x64. A `mono` job builds with the latest Mono (Debian unstable, in a container) through `build/mono/build.sh`, which compiles the same sources with `mcs` because Mono can't load SDK-style projects. It runs only the SQLite and tokenizer tests, since Access needs Windows. A new source folder or resource must be added to that script too. The .NET Framework tests use NUnit 2.6 (`packages/`, run by nunit-console). The .NET tests use NUnit 3 via `dotnet test`, on x64 only. A change that touches only Markdown or agent config skips the Windows build; the `CI result` job still reports.
- .NET Framework references the CLR 2 System.Data.SQLite 1.0.66 in `packages/`; .NET uses the `System.Data.SQLite.Core` package plus the `System.Data.OleDb`/`Odbc` and `System.Configuration.ConfigurationManager` packages.
- `PlaneDisaster.NET Setup` is a WiX 5 SDK project, built after the solution with `dotnet build "PlaneDisaster.NET Setup" -c Release -p:Platform=<x86|x64>`. CI builds the MSIs on the Release legs. WiX 6 and later need the OSMF EULA accepted, so stay on 5 unless the user decides otherwise.
- Releases: bump `AssemblyVersion` in every `AssemblyInfo.cs`, merge, then push a `v<version>` tag. `release.yml` builds and publishes the MSIs and portable zips, and fails if the tag and `AssemblyInfo.cs` disagree.
- On a host without Visual Studio, push a branch and let CI build it.

## Markdown

Every `.md` file must pass markdownlint (`.github/workflows/markdown-lint.yml`). Follow the `markdown-lint` skill, or the `markdown-reviewer` and `markdown-fixer` playbooks. Markdown, YAML and JSON are LF (`.gitattributes`).

## GitHub workflow

- The default branch is `master`. Branch from `origin/master` in a worktree (`git worktree add ../wt-<name> -b <branch> origin/master`). The main checkout belongs to the user.
- Commits end with the attribution trailers the session gives you. PR bodies end with the attribution line the session gives you.
- Wait on CI with `gh pr checks <n> --watch` instead of polling. On failure, read only the failed step: `gh run view <id> --log-failed`.
- `master` requires the `CI result` and `lint` checks. Once the user has asked for a merge, queue it with `gh pr merge <n> --squash --auto --delete-branch` and stop: GitHub merges when the checks pass, so there is nothing to watch. Never use `--admin`.
- Running `gh pr merge` in a worktree prints `fatal: 'master' is already used by worktree` after merging, because it tries to switch the local branch. The merge still happened; confirm with `gh pr view <n> --json state`.
- Action versions are pinned to commit SHAs with the tag in a comment. Dependabot updates them weekly.
