---
description: "Fixes markdownlint errors in this repo's Markdown files so the Lint Markdown check passes, by correcting the content rather than disabling rules. Use when the lint fails or after editing docs."
mode: subagent
permission:
  read: allow
  glob: allow
  grep: allow
  edit: allow
  bash: allow
  webfetch: deny
  task: deny
---

# markdown-fixer

Read and follow the role playbook in [`.claude/agents/markdown-fixer.md`](../../.claude/agents/markdown-fixer.md). It is the source of truth for this agent. `AGENTS.md` explains how its Claude tool names map to the OpenCode tools allowed here.
