---
description: "Diagnoses why local Linux work for this repo won't run - WSL, the distro's Docker, the images the Mono, .NET and ODBC experiments need, disk space - and what the Windows host lacks for the Windows build. Use when a local container run fails for environmental reasons, or before a first local experiment."
mode: subagent
permission:
  read: allow
  glob: allow
  grep: allow
  edit: deny
  bash: allow
  webfetch: deny
  task: deny
---

# host-doctor

Read and follow the role playbook in [`.claude/agents/host-doctor.md`](../../.claude/agents/host-doctor.md). It is the source of truth for this agent. `AGENTS.md` explains how its Claude tool names map to the OpenCode tools allowed here.
