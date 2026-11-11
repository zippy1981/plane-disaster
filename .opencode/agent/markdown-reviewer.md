---
description: "Reviews Markdown files in this repo (docs, Claude agents and skills) against the repo's markdownlint config and for clarity, without changing them. Use before committing doc changes, or when asked to review docs."
mode: subagent
permission:
  read: allow
  glob: allow
  grep: allow
  edit: deny
  bash: allow
  webfetch: deny
  task: allow
---

# markdown-reviewer

Read and follow the role playbook in [`.claude/agents/markdown-reviewer.md`](../../.claude/agents/markdown-reviewer.md). It is the source of truth for this agent. `AGENTS.md` explains how its Claude tool names map to the OpenCode tools allowed here.
