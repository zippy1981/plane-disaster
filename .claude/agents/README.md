# Claude Code agents

This folder holds the [Claude Code subagents](https://docs.claude.com/en/docs/claude-code/sub-agents) for this repo. Each file's YAML front matter sets the agent's `name`, `description`, `tools`, and `model`. The body is its system prompt. They come from [zippy1981/windows-nano-demos](https://github.com/zippy1981/windows-nano-demos), trimmed to what this repo uses.

## Agents

| Agent | Purpose | Model | Tools |
| --- | --- | --- | --- |
| `markdown-reviewer` | Read-only review of Markdown against the lint config. Hands fixes to `markdown-fixer`. | haiku | Bash, Read, Glob, Grep, Agent |
| `markdown-fixer` | Fixes markdownlint errors by editing content, never by disabling rules. | haiku | Bash, Read, Edit, Glob, Grep |

## How models and tools were chosen

- Both agents run a fixed procedure (run markdownlint, fix what it reports), so they use haiku. A bigger model only costs more.
- `markdown-reviewer` keeps Bash only to run markdownlint, and has no Edit tool: it changes files only through `markdown-fixer`.

## Skills the agents use

| Skill | Used by |
| --- | --- |
| `markdown-lint` | `markdown-reviewer`, `markdown-fixer` |

## Delegation

```text
markdown-reviewer ──► markdown-fixer
```

Claude Code subagents can't start other subagents. Running as a subagent, `markdown-reviewer` ends its report by naming the files for `markdown-fixer`.

## OpenCode

`.opencode/agent/<name>.md` mirrors each playbook here with the same description and the permissions its Claude `tools` need. Edit the playbook first, then the wrapper.
