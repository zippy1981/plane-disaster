# Working in this repo

PlaneDisaster.NET is a Windows Forms front end for Access (Jet) and SQLite databases, written in C# against .NET Framework 2.0. The solution is `PlaneDisaster.NET.sln`.

## Other agent tools

- [AGENTS.md](AGENTS.md) carries the same rules for OpenCode and other agents. When a rule changes, change both files in the same commit.
- OpenCode support is [`opencode.json`](opencode.json) (the skill path) plus `.opencode/agent/*.md`, thin wrappers that point at the playbooks in `.claude/agents/`.

## Building

- CI builds the solution in `.github/workflows/build.yml` on `windows-latest`, for Debug and Release, x86 and x64. There is no AnyCPU configuration. A change that touches only Markdown or agent config skips the Windows build; the `CI result` job still reports.
- The projects target .NET Framework 2.0. The workflow gets the reference assemblies from the `Microsoft.NETFramework.ReferenceAssemblies.net20` NuGet package and passes `TargetFrameworkRootPath` and `FrameworkPathOverride` to MSBuild, so the host needs no .NET 3.5 install. Copy those arguments to build locally.
- `packages/` is committed, so build with `-p:RestorePackages=false`; the bundled `.nuget/NuGet.exe` is too old to restore.
- `PlaneDisaster.NET Setup` is a WiX 5 SDK project, built after the solution with `dotnet build "PlaneDisaster.NET Setup" -c Release -p:Platform=<x86|x64>`. CI builds the MSIs on the Release legs. WiX 6 and later need the OSMF EULA accepted, so stay on 5 unless the user decides otherwise.
- Releases: bump `AssemblyVersion` in every `AssemblyInfo.cs`, merge, then push a `v<version>` tag. `release.yml` builds and publishes the MSIs and portable zips, and fails if the tag and `AssemblyInfo.cs` disagree.
- This host has no Visual Studio or .NET SDK. Push a branch and let CI build it.

## Markdown

Every `.md` file must pass markdownlint (`.github/workflows/markdown-lint.yml`). Follow the `markdown-lint` skill, or use the `markdown-reviewer` and `markdown-fixer` agents. Markdown, YAML and JSON are LF (`.gitattributes`).

## GitHub workflow

- The default branch is `master`. Branch from `origin/master` in a worktree (`git worktree add ../wt-<name> -b <branch> origin/master`). The main checkout belongs to the user.
- Commits end with the attribution trailers the session gives you. PR bodies end with the Claude Code line.
- Wait on CI with `gh pr checks <n> --watch` in the background instead of polling. On failure, read only the failed step: `gh run view <id> --log-failed`.
- `master` requires the `CI result` and `lint` checks. Once the user has asked for a merge, queue it with `gh pr merge <n> --squash --auto --delete-branch` and stop: GitHub merges when the checks pass, so there is nothing to watch. Never use `--admin`.
- Running `gh pr merge` in a worktree prints `fatal: 'master' is already used by worktree` after merging, because it tries to switch the local branch. The merge still happened; confirm with `gh pr view <n> --json state`.
- Action versions are pinned to commit SHAs with the tag in a comment. Dependabot updates them weekly.
