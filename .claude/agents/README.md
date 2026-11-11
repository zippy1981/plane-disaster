# Claude Code agents

This folder holds the [Claude Code subagents](https://docs.claude.com/en/docs/claude-code/sub-agents) for this repo. Each file's YAML front matter sets the agent's `name`, `description`, `tools`, and `model`. The body is its system prompt. The Markdown agents come from [zippy1981/windows-nano-demos](https://github.com/zippy1981/windows-nano-demos), and the host tooling from [zippy1981/windows-nano-baseimages](https://github.com/zippy1981/windows-nano-baseimages), both trimmed to what this repo uses.

## Agents

| Agent | Purpose | Model | Tools |
| --- | --- | --- | --- |
| `markdown-reviewer` | Read-only review of Markdown against the lint config. Hands fixes to `markdown-fixer`. | haiku | Bash, Read, Glob, Grep, Agent |
| `markdown-fixer` | Fixes markdownlint errors by editing content, never by disabling rules. | haiku | Bash, Read, Edit, Glob, Grep |
| `host-doctor` | Diagnoses why local Linux work won't run: WSL, the distro's Docker, images, disk. Adapted from `windows-host-doctor`. | haiku | PowerShell, Bash, Read, Glob, Grep |

## How models and tools were chosen

- All three run a fixed procedure (run markdownlint and fix what it reports, or run the host checks), so they use haiku. A bigger model only costs more.
- `markdown-reviewer` keeps Bash only to run markdownlint, and has no Edit tool: it changes files only through `markdown-fixer`.

## Skills the agents use

| Skill | Used by |
| --- | --- |
| `markdown-lint` | `markdown-reviewer`, `markdown-fixer` |
| `host-capabilities` | `host-doctor`, and any session that runs Linux containers (`scripts/Get-HostCapabilities.ps1`, `scripts/host-capabilities.sh`) |
| `safe-docker-cleanup` | Any session that created containers or images (`scripts/Remove-DockerArtifacts.ps1`) |

## Delegation

```text
markdown-reviewer ──► markdown-fixer
```

Claude Code subagents can't start other subagents. Running as a subagent, `markdown-reviewer` ends its report by naming the files for `markdown-fixer`.

## OpenCode

`.opencode/agent/<name>.md` mirrors each playbook here with the same description and the permissions its Claude `tools` need. Edit the playbook first, then the wrapper.
