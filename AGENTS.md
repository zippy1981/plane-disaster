# Working in this repo

PlaneDisaster.NET is a Windows Forms front end for Access (Jet) and SQLite databases, written in C# against .NET Framework 2.0. The solution is `PlaneDisaster.NET.sln`.

This file is the OpenCode (and other agent) counterpart to [CLAUDE.md](CLAUDE.md). The two files carry the same rules. When a rule changes, change both files in the same commit.

## OpenCode in this repo

- **Rules:** OpenCode reads this file as project rules. `opencode.json` adds `.claude/skills` to the skill paths, so the Claude Code skills load as they are. Do not copy a skill into `.opencode/skills`.
- **Agents:** `.opencode/agent/<name>.md` mirrors every `.claude/agents/<name>.md` playbook. The Claude playbook is the source of truth; edit it, then update the wrapper's description and permissions.
- **Models:** OpenCode subagents inherit the session model. To keep the cheap-model split in [.claude/agents/README.md](.claude/agents/README.md), add `model: <provider>/<model-id>` to a wrapper. Bare Claude names (`haiku`, `sonnet`, `opus`) are not OpenCode model IDs.

## Building

- CI builds the solution in `.github/workflows/build.yml` on `windows-latest`, for Debug and Release, x86 and x64. There is no AnyCPU configuration.
- The projects target .NET Framework 2.0. The workflow gets the reference assemblies from the `Microsoft.NETFramework.ReferenceAssemblies.net20` NuGet package and passes `TargetFrameworkRootPath` and `FrameworkPathOverride` to MSBuild, so the host needs no .NET 3.5 install. Copy those arguments to build locally.
- `packages/` is committed, so build with `-p:RestorePackages=false`; the bundled `.nuget/NuGet.exe` is too old to restore.
- `PlaneDisaster.NET Setup` (WiX) isn't built by the solution: it expects SharpDevelop's WiX tools.
- On a host without Visual Studio, push a branch and let CI build it.

## Markdown

Every `.md` file must pass markdownlint (`.github/workflows/markdown-lint.yml`). Follow the `markdown-lint` skill, or the `markdown-reviewer` and `markdown-fixer` playbooks. Markdown, YAML and JSON are LF (`.gitattributes`).

## GitHub workflow

- The default branch is `master`. Branch from `origin/master` in a worktree (`git worktree add ../wt-<name> -b <branch> origin/master`). The main checkout belongs to the user.
- Commits end with the attribution trailers the session gives you. PR bodies end with the attribution line the session gives you.
- Wait on CI with `gh pr checks <n> --watch` instead of polling. On failure, read only the failed step: `gh run view <id> --log-failed`.
- Merge only when the user asked and every check on the head commit passed. Never use `--admin`.
