---
name: host-capabilities
description: Find out whether this host has a Linux container engine (usually a WSL distro's own Docker) and route Linux work to it or to GitHub Actions. Use before any docker command, before running the Mono build, build/mono/check-sources.sh, a .NET SDK command, the Linux ODBC or Jackcess experiments, or shellcheck locally, or when asked whether this machine can run Linux containers.
---

# Host capabilities

Claude Code runs on Windows hosts (some with WSL), on Linux and macOS, inside WSL, and in cloud sandboxes. Don't assume which engines exist. Read the probe's report, then follow the routing table. Adapted from [zippy1981/windows-nano-baseimages](https://github.com/zippy1981/windows-nano-baseimages).

This repo's Windows builds (MSBuild, WiX, the Access Database Engine) only run in CI. Its local container work is all Linux.

## 1. Read the report

| Shell | Command |
| --- | --- |
| PowerShell (Windows) | `./scripts/Get-HostCapabilities.ps1 -AsJson` |
| Bash or sh (any OS; Git Bash hands off to the PowerShell script) | `scripts/host-capabilities.sh` |

A cached report is reused for 24 hours. Probe again (`-Refresh`, or `--refresh`) when a docker command can't reach its engine, when the user says something on the host changed, or before telling the user the host can't do something. On Windows a fresh probe boots WSL distros, one after another, until one has a working Docker.

| Field | Meaning |
| --- | --- |
| `platform` | `windows`, `wsl` (Claude is running inside WSL), `linux` or `macos` |
| `linuxEngine.available`, `.invoke` | Whether a Linux engine answers, and the command that reaches it, for example `["wsl.exe","--distribution","Ubuntu","--exec","docker"]` |
| `linuxEngine.repoPath` | This checkout's path as the Linux engine's host sees it, for bind mounts |
| `linuxEngine.containers` minus `.claudeContainers` | Containers that belong to someone else (the user's devcontainers). Leave them alone. |
| `linuxEngine.reason`, `.tried` | Why there's no Linux engine, and what was tried |
| `windowsEngine.available` | A Windows engine answers. Nothing here uses it, and it may run another repo's CI runners: never touch it. |
| `policy.sharedHost` | `true` unless the user overrode it. Shared means strict cleanup (section 4). |
| `github.authenticated` | Whether `gh` can push and open PRs |

## 2. Route the task

| Task | Run locally when | Otherwise |
| --- | --- | --- |
| Mono build and tests (`build/mono/build.sh`) | `linuxEngine.available`: `debian:sid` with `mono-devel`, as the `mono` CI job does | The `mono` job on a PR |
| Mono source list (`build/mono/check-sources.sh`) | `linuxEngine.available`: `mcr.microsoft.com/dotnet/sdk:11.0` plus `jq` | The `Mono source list` job |
| .NET 11 build or a quick C# check | `linuxEngine.available`: `mcr.microsoft.com/dotnet/sdk:11.0`. It can't run WinForms or the Access tests. | The Windows legs on a PR |
| Linux ODBC experiments (`experiments/linux-odbc/run.sh`, on the `experiment/linux-odbc` branch) | `linuxEngine.available` | Not in CI |
| Windows build, MSI, Access tests | Never locally on this host | The Windows legs on a PR |
| shellcheck | `linuxEngine.available` | Not in CI yet |

## 3. Run on the engine

Use `invoke` exactly as the report gives it, with the docker arguments after it.

```pwsh
$linux = (./scripts/Get-HostCapabilities.ps1).linuxEngine
& $linux.invoke[0] @($linux.invoke | Select-Object -Skip 1) run --rm --name claude-mono-3f9a `
    --label com.zippy1981.created-by=claude-code --label com.zippy1981.session=3f9a `
    --volume "$($linux.repoPath):/src:ro" debian:sid bash -c 'cp -r /src /work && cd /work && ...'
```

- **Git Bash rewrites paths.** `/src` becomes `C:/Program Files/Git/src`, so in Bash prefix the command with `MSYS_NO_PATHCONV=1`. `wsl.exe` output is UTF-16; pipe it through `tr -d '\0'`, or set `WSL_UTF8=1`.
- **`--exec` runs docker without a shell** inside the distro. To run a script there, pass `bash /mnt/c/.../script.sh` as arguments; don't wrap it in `bash -lc '...'`, whose quoting breaks.
- **`/mnt/c` is slow.** Mount the checkout read-only and copy it inside the container (`cp -r /src /work`) before building, which also keeps `bin/` and `obj/` out of the worktree.
- **Line endings:** `.gitattributes` keeps `*.sh` LF, so the scripts run unchanged in Linux from a Windows checkout.

## 4. Name, label and clean up

On a shared host (`policy.sharedHost`, the default), everything you create carries:

- a name `claude-<task>-<tag>`, where `<tag>` is 4 hex characters you pick once per session, or an image tag under a repository you own for the task, such as `plane-disaster-odbc:<driver>`
- `--label com.zippy1981.created-by=claude-code --label com.zippy1981.session=<tag>`, on `docker run`, `docker create` and `docker build`

Prefer images that are already present (`docker images`). If you pull one, note that it wasn't there before, and remove exactly that reference when you're done. Clean up with the `safe-docker-cleanup` skill.

## 5. When an engine is missing

- **No Linux engine, but WSL is installed** (`linuxEngine.tried` names the distros): offer to install Docker Engine in the distro (`sudo apt-get install docker.io`), or to start its daemon (`sudo systemctl start docker`). Both are host changes, so ask first. Otherwise, push and let CI run it.
- **No Linux engine and no WSL:** push and let CI run it, or report what's missing.

## 6. Never

- Switch Docker Desktop between Windows and Linux containers.
- Run `wsl --shutdown` or `wsl --terminate`. They stop the user's devcontainers.
- Start, stop or reconfigure a daemon, or install software on the host, without asking. Installing packages inside your own container is fine.
- Touch containers or images you didn't create, on either engine.

## Overrides

Per host, in `.claude/host-capabilities.overrides.local.json`, which is gitignored:

```json
{
  "linuxEngine": "wsl:Ubuntu",
  "sharedHost": true,
  "maxAgeHours": 24
}
```

- `linuxEngine`: `auto` (the default), `none`, `wsl:<distro>` (Windows only), `docker` (the default docker context, if it's a Linux engine), or `docker-desktop-linux` (Windows only)
- `sharedHost`: set `false` only on a host that nothing else uses
- `maxAgeHours`: how long a report is reused

Reports are kept in `.claude/host-capabilities.<platform>.local.json`, one per platform, so a Windows session and a WSL session on the same checkout don't overwrite each other.
